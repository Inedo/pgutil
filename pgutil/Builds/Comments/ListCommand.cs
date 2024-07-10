using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class CommentsCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "List comments";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await foreach (var p in client.ListCommentsAsync(context.GetOption<ProjectOption>(), context.GetOption<BuildOption>(), cancellationToken))
                        Console.WriteLine($"#{p.Number} - {p.Comment}");

                    return 0;
                }
            }
        }
    }
}
