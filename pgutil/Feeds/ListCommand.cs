using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "List feeds";

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
