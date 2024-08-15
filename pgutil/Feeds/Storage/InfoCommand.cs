using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class StorageCommand
        {
            private sealed class InfoCommand : IConsoleCommand
            {
                public static string Name => "info";
                public static string Description => "Display storage configuration for a feed";
                public static string Examples => """
                      $> pgutil feeds storage info --feed=approved-nuget

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-storage-get
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<FeedOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = context.GetFeedName();

                    var config = await client.GetFeedStorageConfigurationAsync(feed, cancellationToken);
                    CM.WriteLine("Type: ", new TextSpan(config.Id, ConsoleColor.White));
                    foreach (var prop in config.Properties)
                        CM.WriteLine($"{prop.Key}: {prop.Value ?? "<default>"}");

                    return 0;
                }
            }
        }
    }
}
