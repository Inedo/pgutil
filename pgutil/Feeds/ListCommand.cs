using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists feeds";
            public static string Examples => """
                  $> pgutil feeds list

                  $> pgutil feeds list --inactive

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-list
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<InactiveFlag>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                bool includeInactive = context.HasFlag<InactiveFlag>();
                await foreach (var feed in client.ListFeedsAsync(cancellationToken))
                {
                    if (includeInactive || feed.Active == true)
                        Console.WriteLine($"{feed.Name} ({feed.FeedType})");
                }

                return 0;
            }

            private sealed class InactiveFlag : IConsoleFlagOption
            {
                public static string Name => "--inactive";
                public static string Description => "Include inactive feeds in the displayed results";
            }
        }
    }
}
