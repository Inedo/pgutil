using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class DeleteCommand : IConsoleCommand
        {
            public static string Name => "delete";
            public static string Description => "Deletes a build";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<ProjectOption>()
                    .WithOption<BuildOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var project = context.GetOption<ProjectOption>();
                var build = context.GetOption<BuildOption>();
                CM.WriteLine($"Deleting build {build} from {project}...");
                await client.DeleteBuildAsync(project, build, cancellationToken);
                CM.WriteLine("Build deleted.");
                return 0;
            }
        }
    }
}
