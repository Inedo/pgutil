using System.Text.RegularExpressions;
using ConsoleMan;
using Microsoft.Extensions.FileSystemGlobbing;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed partial class UploadCommand : IConsoleCommand
        {
            public static string Name => "upload";
            public static string Description => "Uploads a package file to ProGet.";
            public static string Examples => """
                  $> pgutil packages upload --feed=approved-nuget --input-file=C:\development\nuget_packages\Newtonsoft.Json.13.0.3.nupkg
                  $> pgutil packages upload --feed=public-npm --input-file=C:\packages\npm_packages\package.tgz
                  $> pgutil packages upload --feed=approved-debian --input-file=C:\projects\project-packages\debhelper_13.15.3_all.deb --distribution=main
                  $> pgutil packages upload --feed=internal-maven --input-file=my-app-1.1.jar --artifactPath=/com/my-company/my-app/1.1
                  $> pgutil packages upload --feed=internal-rpm --input-file=*.rpm

                For more information, see: https://docs.inedo.com/docs/proget/api/packages/upload
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<InputFileOption>()
                    .WithOption<StdInFlag>()
                    .WithOption<DistributionOption>()
                    .WithOption<ComponentOption>()
                    .WithOption<ArtifactPathOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var feed = context.GetFeedName();
                bool stdin = context.HasFlag<StdInFlag>();

                int downloadedFiles = 0;
                var inputFileNames = getSourceFileNames();
                do
                {
                    using var source = getSource(out var fileName);
                    downloadedFiles++;

                    if (!string.IsNullOrEmpty(fileName))
                        fileName = Path.GetFileName(fileName);

                    if (context.TryGetOption<ArtifactPathOption>(out var artifactPath))
                        fileName = $"{artifactPath.TrimEnd('/')}/{fileName}";

                    var distribution = context.GetOptionOrDefault<DistributionOption>();
                    var component = context.GetOptionOrDefault<ComponentOption>();

                    if (!Console.IsOutputRedirected && source.CanSeek)
                    {
                        long length = source.Length;

                        using var progress = ProgressWriter.Create(0, length, (v, w) =>
                        {
                            w.WriteSize(v);
                            w.Write("/");
                            w.WriteSize(length);
                        });

                        await client.UploadPackageAsync(source, feed, fileName, distribution, component, progress.SetCurrentValue, cancellationToken);
                        progress.Completed();
                    }
                    else
                    {
                        await client.UploadPackageAsync(source, feed, distribution: distribution, component: component, cancellationToken: cancellationToken);
                    }

                    Console.WriteLine("Upload complete.");
                }
                while (inputFileNames?.Count > 0);

                return 0;

                Stream getSource(out string? fileName)
                {
                    if (stdin)
                    {
                        if (inputFileNames is not null)
                        {
                            CM.WriteError<InputFileOption>("Input file name is cannot be used with --stdin.");
                            context.WriteUsage();
                            throw new PgUtilException();
                        }

                        fileName = null;
                        CM.WriteLine("Uploading package from ", new TextSpan("<stdin>", ConsoleColor.White), " to ", new TextSpan(feed, ConsoleColor.White), " feed...");
                        return Console.OpenStandardInput();
                    }
                    else
                    {
                        if (inputFileNames is null)
                        {
                            CM.WriteError<InputFileOption>("Input file name is required when not using --stdin.");
                            context.WriteUsage();
                            throw new PgUtilException();
                        }

                        if (inputFileNames.Count == 0 && downloadedFiles == 0)
                        {
                            CM.WriteError<InputFileOption>("No files matching the search expression were found.");
                            throw new PgUtilException();
                        }

                        var inputFileName = inputFileNames[0];
                        inputFileNames.RemoveAt(0);

                        if (!File.Exists(inputFileName))
                        {
                            CM.WriteError<InputFileOption>($"{inputFileName} not found.");
                            throw new PgUtilException();
                        }

                        fileName = inputFileName;
                        CM.WriteLine("Uploading ", new TextSpan(inputFileName, ConsoleColor.White), " to ", new TextSpan(feed, ConsoleColor.White), " feed...");
                        return File.Open(inputFileName, new FileStreamOptions { Access = FileAccess.Read, Mode = FileMode.Open, Options = FileOptions.SequentialScan | FileOptions.Asynchronous });
                    }
                }

                List<string>? getSourceFileNames()
                {
                    if (context.TryGetOption<InputFileOption>(out var inputFileName))
                    {
                        if (!inputFileName.Contains('*'))
                            return [inputFileName];

                        var fullPath =  Path.GetFullPath(inputFileName);
                        int firstWildcardIndex = FirstWildcardRegex().Match(fullPath).Index;
                        var rootPath = fullPath[..firstWildcardIndex];
                        var wildcardPart = fullPath[(firstWildcardIndex)..];

                        var matcher = new Matcher(OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                        matcher.AddInclude(wildcardPart.Replace('\\', '/'));
                        return [.. matcher.GetResultsInFullPath(rootPath)];
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            [GeneratedRegex(@"(?<=[/\\])[^/\\]*\*")]
            internal static partial Regex FirstWildcardRegex();

            private sealed class InputFileOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--input-file";
                public static string Description => "Name of the file to upload. Wildcards are allowed.";
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
                public static string Description => "Distribution of the package. Only applies to Debian packages";
            }

            private sealed class ComponentOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--component";
                public static string Description => "Component of the package. Only applies to Debian packages (default is main)";
            }

            private sealed class ArtifactPathOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--artifactPath";
                public static string Description => "Artifact path in the feed; only applies to Maven artifacts (required)";
            }
        }
    }
}
