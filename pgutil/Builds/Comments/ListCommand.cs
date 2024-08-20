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
                public static string Description => "Lists comments";
                public static string Examples => """
                      >$ pgutil builds comments list --project=testProject --build=2.2.4

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/issues/proget-api-sca-comments-list
                    """;

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
