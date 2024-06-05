using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
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
    }
}
