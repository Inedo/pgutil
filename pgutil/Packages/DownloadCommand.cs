using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class DownloadCommand : IConsoleCommand
        {
            public static string Name => "download";
            public static string Description => "Download a package file from ProGet";
            public static string Examples => """
                 > pgutil packages download --feed=public-nuget --package=Newtonsoft.Json --version=13.0.3 --output-file=c:\myPackages\NuGetPackages\Newtonsoft.Json.13.0.3.nupkg 
                 > pgutil packages download --feed=approved-npm --package=@babel/runtime --version=7.25.0 --output-file=C:\npm-packages\package.tgz 
                 > pgutil packages download --feed=public-debian --package= debhelper --version=13.15.3 --component=main --distro=stable --arch=all --output-file=C:\debian-packages\debhelper_13.15.3_all.deb 

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-packages/proget-api-packages-download 
                """;

            public static void Configure(ICommandBuilder builder)
            {
                WithPackageOptions(builder)
                    .WithOption<OutputFileOption>()
                    .WithOption<StdOutFlag>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var feedInfo = await client.GetBasicFeedInfoAsync(context.GetFeedName(), cancellationToken);
                var (package, fullName) = GetPackageIdentifier(context);

                QualifierOptions.EnsureQualifiersForPackageType(feedInfo.PackageType, package.Qualifier);

                bool stdout = context.HasFlag<StdOutFlag>();

                if (!stdout)
                {
                    CM.Write("Downloading ", new TextSpan($"{fullName} {package.Version}", ConsoleColor.White));
                    if (!string.IsNullOrWhiteSpace(package.Qualifier))
                        CM.Write(ConsoleColor.White, $" {package.Qualifier}");

                    CM.WriteLine(" from ", new TextSpan(package.Feed, ConsoleColor.White), " feed...");
                }

                using var source = await client.DownloadPackageAsync(package, cancellationToken);
                if (!context.TryGetOption<OutputFileOption>(out var fileName) && !stdout)
                {
                    if (!string.IsNullOrWhiteSpace(source.FileName))
                    {
                        fileName = source.FileName;
                    }
                    else
                    {
                        CM.WriteError<OutputFileOption>("Output file not specified and server did not return a default file name.");
                        return -1;
                    }
                }

                if (stdout && !string.IsNullOrEmpty(fileName))
                {
                    CM.WriteError<OutputFileOption>("Cannot specify both --stdout and --output-file.");
                    return -1;
                }

                if (!stdout)
                {
                    CM.WriteLine("Saving package to ", new TextSpan(fileName, ConsoleColor.White), "...");

                    using var fileStream = File.Open(fileName!, new FileStreamOptions { Access = FileAccess.Write, Mode = FileMode.Create, Options = FileOptions.Asynchronous });

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
            }

            private sealed class OutputFileOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--output-file";
                public static string Description => "Name of the file to save local package as. The server-provided value is used if this is not specified.";
            }

            private sealed class StdOutFlag : IConsoleFlagOption
            {
                public static string Name => "--stdout";
                public static string Description => "Write the package to stdout instead of a file";
            }
        }
    }
}
