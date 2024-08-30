using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class ProjectsCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "Lists projects";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await foreach (var p in client.ListProjectsAsync(cancellationToken))
                    {
                        Console.Write(p.Name);
                        if (!string.IsNullOrEmpty(p.Type))
                            Console.Write($" ({p.Type})");
                        Console.WriteLine();
                    }

                    return 0;
                }
            }
        }
    }
}
