using ConsoleMan;
using Inedo.DependencyScan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class SbomCommand : IConsoleCommand
        {
            public static string Name => "sbom";
            public static string Description => "Generates a minimal SBOM document";
            public static bool Undisclosed => true;
            public static string Examples => """
                  $> pgutil builds sbom --input=WebApplicationTool.csproj --output=C:\pgutil\output.xml --project-name="Web Application Tool" --version=1.2.3 

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/sbom/proget-api-sca-sbom-export
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<InputOption>()
                    .WithOption<OutputOption>()
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

                var output = context.GetOption<OutputOption>();

                var consumer = new PackageConsumer
                {
                    Name = context.GetOption<ProjectNameOption>(),
                    Version = context.GetOption<VersionOption>()
                };

                CM.WriteLine("Writing ", new TextSpan(output, ConsoleColor.White), "...");
                BomWriter.WriteSbom(output, projects, consumer, context.GetOption<ProjectTypeOption>(), scanner.Type.ToString().ToLowerInvariant(), true);

                CM.WriteLine(new TextSpan(output, ConsoleColor.White), " written.");
                return 0;
            }

            private sealed class OutputOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--output";
                public static string Description => "Output sbom document";
            }
        }
    }
}
