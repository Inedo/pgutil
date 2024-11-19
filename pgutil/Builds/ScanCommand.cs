using ConsoleMan;
using Inedo.DependencyScan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class ScanCommand : IConsoleCommand
        {
            public static string Name => "scan";
            public static string Description => "Generates a minimal SBOM from project dependencies and uploads it to ProGet";
            public static string Examples => """
                  $> pgutil builds scan --project-name="Web Data Tool" --version=1.2.3

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/builds/proget-api-sca-builds-scan
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<ProjectNameOption>()
                    .WithOption<VersionOption>()
                    .WithOption<InputOption>()
                    .WithOption<ProjectTypeOption>()
                    .WithOption<IncludeProjectReferencesFlag>()
                    .WithOption<IncludeDevDependenciesFlag>()
                    .WithOption<DoNotScanNodeModulesFlag>()
                    .WithOption<ScannerTypeOption>()
                    .WithOption<DoNotAuditFlag>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var input = context.GetOption<InputOption>();

                CM.WriteLine("Scanning for dependencies in ", new TextSpan(input, ConsoleColor.White), "...");
                var scannerType = Enum.TryParse<DependencyScannerType>(context.GetOption<ScannerTypeOption>(), out var _type) ? _type : DependencyScannerType.Auto;
                var scanner = await DependencyScanner.GetScannerAsync(new CreateDependencyScannerArgs(
                    input ?? string.Empty, 
                    SourceFileSystem.Default, 
                    IncludeProjectReferences: context.HasFlag<IncludeProjectReferencesFlag>(), 
                    DoNotScanNodeModules: context.HasFlag<DoNotScanNodeModulesFlag>(),
                    IncludeDevDependencies: context.HasFlag<IncludeDevDependenciesFlag>()
                ), scannerType);
                var projects = await scanner.ResolveDependenciesAsync(cancellationToken);
                if (projects.Count == 0)
                {
                    CM.WriteError($"No projects found in input path: {input ?? Environment.CurrentDirectory}");
                    return -1;
                }

                var consumer = new PackageConsumer
                {
                    Name = context.GetOption<ProjectNameOption>(),
                    Version = context.GetOption<VersionOption>()
                };

                var client = context.GetProGetClient();

                CM.WriteLine("Publishing SBOM to ProGet...");
                await client.PublishSbomAsync(projects, consumer, context.GetOption<ProjectTypeOption>(), scanner.Type.ToString().ToLowerInvariant(), cancellationToken);
                CM.WriteLine("SBOM published.");

                if(context.HasFlag<DoNotAuditFlag>())
                    return 0;

                CM.WriteLine("Auditing ", new TextSpan($"{consumer.Name} {consumer.Version}", ConsoleColor.White), "...");
                var results = await client.AuditBuildAsync(consumer.Name, consumer.Version, cancellationToken);
                if (results.LastAnalyzedDate.HasValue)
                {
                    CM.WriteLine("Analyzed: ", new TextSpan(results.LastAnalyzedDate.Value.ToLocalTime().ToString(), ConsoleColor.White));
                    CM.WriteLine(
                        "Status: ",
                        new TextSpan(
                            results.StatusText,
                            results.StatusCode switch
                            {
                                "N" when results.UnresolvedIssueCount == 0 => ConsoleColor.Blue,
                                "N" => ConsoleColor.Red,
                                "W" => ConsoleColor.DarkYellow,
                                "I" => ConsoleColor.Yellow,
                                _ => ConsoleColor.Green
                            }
                        )
                    );

                    return results.StatusCode switch
                    {
                        "N" when results.UnresolvedIssueCount == 0 => 0,
                        "N" => -1,
                        _ => 0
                    };
                }
                else
                {
                    // this should not happen
                    CM.WriteError("ProGet reported that the build was not analyzed.");
                    return -1;
                }
            }
        }
    }
}
