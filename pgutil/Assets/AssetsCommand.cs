using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed class AssetsCommand : IConsoleCommandContainer
    {
        public static string Name => "assets";
        public static string Description => "Work with a ProGet asset directory";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithOption<FeedOption>()
                .WithCommand<ListCommand>()
                .WithCommand<DownloadCommand>()
                .WithCommand<UploadCommand>()
                .WithCommand<MkDirCommand>()
                .WithCommand<RmCommand>();
        }

        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists assets at the specified path";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PathOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetAssetDirectoryClient();
                var items = (await client.ListContentsAsync(context.GetOptionOrDefault<PathOption>(), cancellationToken: cancellationToken).ToListAsync())
                    .OrderByDescending(i => i.Directory)
                    .ThenBy(i => i.Name);

                foreach (var item in items)
                {
                    if (item.Size.HasValue)
                        CM.WriteLine($"{item.Name,-50} {item.Size,14}");
                    else
                        CM.WriteLine(ConsoleColor.Blue, $"{item.Name,-50} {"<DIR>",14}");
                }

                return 0;
            }

            private sealed class PathOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--path";
                public static string Description => "Root path to list items in";
            }
        }

        private sealed class DownloadCommand : IConsoleCommand
        {
            public static string Name => "download";
            public static string Description => "Downloads an item to a file";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PathOption>()
                    .WithOption<OutputOption>()
                    .WithOption<StdOutFlag>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetAssetDirectoryClient();

                bool stdout = context.HasFlag<StdOutFlag>();

                var path = context.GetOption<PathOption>().Replace('\\', '/');

                if (context.TryGetOption<OutputOption>(out var output))
                {
                    if (stdout)
                    {
                        CM.WriteError<OutputOption>("Cannot specify both --stdout and --output.");
                        return -1;
                    }

                    if (output.EndsWith('/') || output.EndsWith('\\') || Directory.Exists(output))
                        output = Path.Combine(output, GetFileName(path));
                }
                else
                {
                    output = GetFileName(path);
                }

                if (!stdout)
                {
                    CM.Write("Downloading ", new TextSpan(path, ConsoleColor.White));
                    CM.WriteLine(" from ", new TextSpan(client.AssetDirectoryName, ConsoleColor.White), "...");
                }

                using var source = await client.DownloadFileAsync(path, cancellationToken);

                if (!stdout)
                {
                    CM.WriteLine("Saving to ", new TextSpan(output, ConsoleColor.White), "...");

                    using var fileStream = File.Open(output, new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Create, Options = FileOptions.Asynchronous });

                    if (!Console.IsOutputRedirected && source.ContentLength.HasValue)
                    {
                        using var progress = ProgressWriter.Create(0, source.ContentLength.Value, (v, w) =>
                        {
                            w.WriteSize(v);
                            w.Write("/");
                            w.WriteSize(source.ContentLength.GetValueOrDefault());
                        });
                        source.BytesReadChanged += (s, e) => progress.SetCurrentValue(source.BytesRead);

                        await source.CopyToAsync(fileStream, cancellationToken);

                        progress.Completed();
                    }

                    CM.WriteLine($"Download complete ({source.BytesRead:N0} bytes)");
                }
                else
                {
                    using var outStream = Console.OpenStandardOutput();
                    await source.CopyToAsync(outStream, cancellationToken);
                }

                return 0;

                static string GetFileName(string s)
                {
                    int index = s.LastIndexOf('/');
                    if (index < 0)
                        return s;
                    else
                        return s[(index + 1)..];
                }
            }

            private sealed class PathOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--path";
                public static string Description => "Path of item to download";
            }

            private sealed class OutputOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--output";
                public static string Description => "File or directory name to save the asset item";
            }

            private sealed class StdOutFlag : IConsoleFlagOption
            {
                public static string Name => "--stdout";
                public static string Description => "Write the asset to stdout instead of a file";
            }
        }

        private sealed class UploadCommand : IConsoleCommand
        {
            public static string Name => "upload";
            public static string Description => "Uploads a file to an asset directory";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<TargetPathOption>()
                    .WithOption<FileOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetAssetDirectoryClient();

                var target = context.GetOption<TargetPathOption>();

                using var source = getSource();

                if (!Console.IsOutputRedirected && source.CanSeek)
                {
                    long length = source.Length;

                    using var progress = ProgressWriter.Create(0, length, (v, w) =>
                    {
                        w.WriteSize(v);
                        w.Write("/");
                        w.WriteSize(length);
                    });

                    await upload(progress.SetCurrentValue);
                    progress.Completed();
                }
                else
                {
                    await upload(null);
                }

                Console.WriteLine("Upload complete.");
                return 0;

                Stream getSource()
                {
                    var inputFileName = context.GetOption<FileOption>();

                    if (!File.Exists(inputFileName))
                    {
                        CM.WriteError<FileOption>($"{inputFileName} not found.");
                        throw new PgUtilException();
                    }

                    CM.WriteLine("Uploading ", new TextSpan(inputFileName, ConsoleColor.White), " to ", new TextSpan(client.AssetDirectoryName, ConsoleColor.White), " feed...");
                    return File.Open(inputFileName, new FileStreamOptions { Access = FileAccess.Read, Mode = FileMode.Open, Options = FileOptions.SequentialScan | FileOptions.Asynchronous });
                }

                async Task upload(Action<long>? reportProgress)
                {
                    int partSize = context.GetOption<PartSizeOption, int>();
                    if (partSize < 1)
                    {
                        CM.WriteError<PartSizeOption>("part size must be at least 1");
                        throw new PgUtilException();
                    }

                    if (source.Length <= partSize)
                        await client.UploadFileAsync(target, source, context.GetOptionOrDefault<ContentTypeOption>(), reportProgress, cancellationToken);
                    else
                        await client.UploadMultipartFileAsync(target, source, source.Length, context.GetOptionOrDefault<ContentTypeOption>(), partSize * 1024 * 1024, reportProgress, cancellationToken);
                }
            }

            private sealed class TargetPathOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--target-path";
                public static string Description => "Target location in asset directory including file name";
            }

            private sealed class FileOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--file";
                public static string Description => "Name of file to upload";
            }

            private sealed class PartSizeOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--partsize";
                public static string Description => "Chunk size (in mb) for multipart uploads";
                public static string DefaultValue => "5";
            }

            private sealed class ContentTypeOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--content-type";
                public static string Description => "Content-Type of the item to upload";
            }
        }

        private sealed class MkDirCommand : IConsoleCommand
        {
            public static string Name => "mkdir";
            public static string Description => "Creates a subdirectory in an asset directory";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PathOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetAssetDirectoryClient();
                var path = context.GetOption<PathOption>();
                CM.WriteLine("Creating ", new TextSpan(path, ConsoleColor.White), " on ", client.AssetDirectoryName, "...");
                await client.CreateDirectoryAsync(path, cancellationToken);
                CM.WriteLine(new TextSpan(path, ConsoleColor.White), " created!");
                return 0;
            }

            private sealed class PathOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--path";
                public static string Description => "Path of subdirectory to create. It is not an error if the subdirectory already exists";
            }
        }

        private sealed class RmCommand : IConsoleCommand
        {
            public static string Name => "rm";
            public static string Description => "Deletes a file or subdirectory in an asset directory";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PathOption>()
                    .WithOption<ForceOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetAssetDirectoryClient();
                var path = context.GetOption<PathOption>();
                CM.WriteLine("Deleting ", new TextSpan(path, ConsoleColor.White), " from ", client.AssetDirectoryName, "...");
                await client.DeleteItemAsync(path, context.HasFlag<ForceOption>(), cancellationToken);
                CM.WriteLine(new TextSpan(path, ConsoleColor.White), " deleted.");
                return 0;
            }

            private sealed class PathOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--path";
                public static string Description => "Path of item to delete. It is not an error if the item doesn't exist";
            }

            private sealed class ForceOption : IConsoleFlagOption
            {
                public static string Name => "--force";
                public static string Description => "Deletes a subdirectory even if it is not empty";
            }
        }
    }
}
