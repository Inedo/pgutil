using System.Net;
using System.Text.RegularExpressions;
using ConsoleMan;
using Inedo.DependencyScan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class ScanCommand : IConsoleCommand
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
               context.TryGetOption<InputOption>(out var input);

                CM.WriteLine("Scanning for dependencies in ", new TextSpan(input ?? Environment.CurrentDirectory, ConsoleColor.White), "...");
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

                return await ExecuteAudit(client, consumer, cancellationToken);
            }

            //Copy paste from AuditCommand.cs
            private static async Task<int> ExecuteAudit(ProGetClient client, PackageConsumer consumer, CancellationToken cancellationToken)
            {
                try 
                { 
                    var project = consumer.Name;
                    var build = consumer.Version;

                    CM.WriteLine("Auditing ", new TextSpan($"{project} {build}", ConsoleColor.White), "...");
                    CM.WriteLine();

                    var results = await client.AuditBuildAsync(project, build, cancellationToken);
                    if (results.LastAnalyzedDate.HasValue)
                    {
                        var buildInfo = await client.GetBuildAsync(project, build, cancellationToken);

                        if (buildInfo.Created.HasValue)
                            CM.WriteLine("Created: ", buildInfo.Created.Value.ToLocalTime().ToString());

                        CM.WriteLine("Status: ", buildInfo.Active ? "Active" : new TextSpan("Archived", ConsoleColor.DarkGray));

                        CM.WriteLine("Release: ", buildInfo.Release ?? "-");

                        if (buildInfo.Stage is not null)
                            CM.WriteLine("Stage: ", buildInfo.Stage);

                        CM.Write("Last Analysis: ",
                            new TextSpan(
                                results.StatusText,
                                results.StatusCode switch
                                {
                                    "N" => ConsoleColor.Red,
                                    "W" => ConsoleColor.DarkYellow,
                                    "I" => ConsoleColor.Yellow,
                                    _ => ConsoleColor.Green
                                }
                            )
                        );

                        if (results.IssueCount > 0 && results.IssueCount == results.UnresolvedIssueCount)
                            CM.Write(" (", new TextSpan("Resolved", ConsoleColor.Blue), ") ");

                        CM.WriteLine(" on ", results.LastAnalyzedDate.GetValueOrDefault().ToLocalTime().ToString());

                        if (results.TotalPackages.HasValue)
                            CM.WriteLine("Total Packages: ", results.TotalPackages.Value.ToString());

                        CM.WriteLine();

                        if (buildInfo.Packages?.Length > 0)
                        {
                            CM.WriteLine(ConsoleColor.White, "-= Packages =-");
                            CM.WriteLine();

                            foreach (var p in buildInfo.Packages)
                            {
                                var m = PurlRegex().Match(p.PUrl);
                                if (!m.Success)
                                    CM.WriteLine(p.PUrl);
                                else
                                    CM.WriteLine(ConsoleColor.White, $"{m.Groups[1].ValueSpan} {m.Groups[2].ValueSpan}");

                                CM.WriteLine(" Compliance: ", p.Compliance.Result);
                                CM.Write(" License: ");
                                if (p.Licenses.Length == 0)
                                    CM.WriteLine("None");
                                else
                                    CM.WriteLine(string.Join(", ", p.Licenses));

                                CM.Write(" Vulnerabilities: ");
                                if (p.Vulnerabilities?.Length > 0)
                                {
                                    CM.WriteLine(string.Join(", ", p.Vulnerabilities.Select(p => $"{p.Id} ({p.Score})")));
                                    foreach (var v in p.Vulnerabilities)
                                        CM.WriteLine("  ", v.Title);
                                }
                                else
                                {
                                    CM.WriteLine("None");
                                }

                                CM.WriteLine();
                            }
                        }

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
                    catch (ProGetApiException ex) when(ex.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    CM.WriteError("ProGet Basic rate limit exceeded.");
                    return 429;
                }
            }

            [GeneratedRegex(@"^pkg:[^/]+/(?<1>[^@]+)@(?<2>[^?]+)")]
            public static partial Regex PurlRegex();
        }
    }
}
