using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed class DeleteCommand : IConsoleCommand
        {
            public static string Name => "delete";
            public static string Description => "Deletes a feed";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<FeedOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feedName = context.GetOption<FeedOption>();

                CM.WriteLine("Deleting ", new TextSpan(feedName, ConsoleColor.White), "...");
                await client.DeleteFeedAsync(feedName, cancellationToken);
                Console.WriteLine("Feed deleted.");
                return 0;
            }
        }
    }
}
