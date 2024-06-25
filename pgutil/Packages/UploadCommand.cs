using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class UploadCommand : IConsoleCommand
        {
            public static string Name => "upload";
            public static string Description => "Upload a package file to ProGet.";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<InputFileOption>()
                    .WithOption<StdInFlag>()
                    .WithOption<DistributionOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var feed = context.GetFeedName();
                bool stdin = context.HasFlag<StdInFlag>();

                using var source = getSource();

                var fileName = context.GetOptionOrDefault<InputFileOption>();
                if (!string.IsNullOrEmpty(fileName))
                    fileName = Path.GetFileName(fileName);

                if (!Console.IsOutputRedirected && source.CanSeek)
                {
                    long length = source.Length;

                    using var progress = ProgressWriter.Create(0, length, (v, w) =>
                    {
                        w.WriteSize(v);
                        w.Write("/");
                        w.WriteSize(length);
                    });

                    await client.UploadPackageAsync(source, feed, fileName, context.GetOptionOrDefault<DistributionOption>(), progress.SetCurrentValue, cancellationToken);
                    progress.Completed();
                }
                else
                {
                    await client.UploadPackageAsync(source, feed, cancellationToken: cancellationToken);
                }

                Console.WriteLine("Upload complete.");
                return 0;

                Stream getSource()
                {
                    if (stdin)
                    {
                        if (context.TryGetOption<InputFileOption>(out _))
                        {
                            CM.WriteError<InputFileOption>("Input file name is cannot be used with --stdin.");
                            context.WriteUsage();
                            throw new PgUtilException();
                        }

                        CM.WriteLine("Uploading package from ", new TextSpan($"<stdin>", ConsoleColor.White), " to ", new TextSpan(feed, ConsoleColor.White), " feed...");
                        return Console.OpenStandardInput();
                    }
                    else
                    {
                        if (!context.TryGetOption<InputFileOption>(out var inputFileName))
                        {
                            CM.WriteError<InputFileOption>("Input file name is required when not using --stdin.");
                            context.WriteUsage();
                            throw new PgUtilException();
                        }

                        if (!File.Exists(inputFileName))
                        {
                            CM.WriteError<InputFileOption>($"{inputFileName} not found.");
                            throw new PgUtilException();
                        }

                        CM.WriteLine("Uploading ", new TextSpan(inputFileName, ConsoleColor.White), " to ", new TextSpan(feed, ConsoleColor.White), " feed...");
                        return File.Open(inputFileName, new FileStreamOptions { Access = FileAccess.Read, Mode = FileMode.Open, Options = FileOptions.SequentialScan | FileOptions.Asynchronous });
                    }
                }
            }

            private sealed class InputFileOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--input-file";
                public static string Description => "Name of the file to upload.";
            }

            private sealed class StdInFlag : IConsoleFlagOption
            {
                public static string Name => "--stdin";
                public static string Description => "Read the package from stdin instead of a file.";
            }

            private sealed class DistributionOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--distribution";
                public static string Description => "Distribution of the package. Only applies to Debian packages (default is main)";
            }
        }
    }
}
