using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class ProjectsCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a project";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<ProjectOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var project = context.GetOption<ProjectOption>();
                    CM.WriteLine($"Deleting project {project}...");
                    await client.DeleteProjectAsync(project, cancellationToken);
                    CM.WriteLine("Project deleted.");
                    return 0;
                }
            }
        }
    }
}
