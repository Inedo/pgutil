using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class IssuesCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "List issues";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await foreach (var p in client.ListIssuesAsync(context.GetOption<ProjectOption>(), context.GetOption<BuildOption>(), cancellationToken))
                    {
                        Console.Write($"#{p.Number} - {p.Detail}");
                        if (p.Resolved)
                            CM.WriteLine(ConsoleColor.Green, " [resolved]");
                        else
                            Console.WriteLine();
                    }

                    return 0;
                }
            }
        }
    }
}
