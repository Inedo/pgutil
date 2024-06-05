using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed class UploadCommand : IConsoleCommand
        {
            public static string Name => "upload";
            public static string Description => "Uploads a file to an asset directory";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<TargetPathOption>()
                    .WithOption<FileOption>()
                    .WithOption<PartSizeOption>();
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
    }
}
