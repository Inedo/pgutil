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
            public static string Description => "Generate a minimal SBOM from project dependencies and upload it to ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<InputOption>()
                    .WithOption<ProjectNameOption>()
                    .WithOption<VersionOption>()
                    .WithOption<ProjectTypeOption>()
                    .WithOption<IncludeProjectReferencesFlag>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var input = context.GetOption<InputOption>();

                CM.WriteLine("Scanning for dependencies in ", new TextSpan(input, ConsoleColor.White), "...");

                var scanner = DependencyScanner.GetScanner(new CreateDependencyScannerArgs(input, SourceFileSystem.Default, IncludeProjectReferences: context.HasFlag<IncludeProjectReferencesFlag>()));
                var projects = await scanner.ResolveDependenciesAsync(cancellationToken);
                if (projects.Count == 0)
                {
                    CM.WriteError($"No projects found in input path: {input}");
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

                return 0;
            }
        }
    }
}
