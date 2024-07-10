using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class ProjectsCommand
        {
            private sealed class InfoCommand : IConsoleCommand
            {
                public static string Name => "info";
                public static string Description => "Display information about a project";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<ProjectOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var p = await client.GetProjectAsync(context.GetOption<ProjectOption>(), cancellationToken);
                    Console.Write(p.Name);
                    if (!string.IsNullOrEmpty(p.Type))
                        Console.Write($" ({p.Type})");
                    Console.WriteLine();

                    return 0;
                }

                private sealed class ProjectOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--project";
                    public static string Description => "Name of the project";
                }
            }
        }
    }
}
