using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class PropertiesCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "List properties of a feed";
                public static string Examples => """
                      $> pgutil feeds properties list --feed=myNugetFeed

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-get
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = await client.GetFeedAsync(context.GetFeedName(), cancellationToken);

                    WriteProperty("alternateNames", feed.AlternateNames);
                    WriteProperty("feedType", feed.FeedType);
                    WriteProperty("active", feed.Active);
                    WriteProperty("dropPath", feed.DropPath);
                    WriteProperty("endpointUrl", feed.EndpointUrl);
                    WriteProperty("connectors", feed.Connectors);
                    WriteProperty("canPublish", feed.CanPublish);
                    WriteProperty("vulnerabilitiesEnabled", feed.VulnerabilitiesEnabled);

                    return 0;
                }

                private static void WriteProperty(string name, string? value)
                {
                    CM.Write(new TextSpan(name, ConsoleColor.White), new TextSpan("=", ConsoleColor.Yellow));
                    if (value is null)
                        CM.Write(ConsoleColor.DarkGray, "*not set*");
                    else
                        CM.Write(ConsoleColor.Blue, value);

                    Console.WriteLine();
                }
                private static void WriteProperty(string name, string[]? values) => WriteProperty(name, values?.Length > 0 ? string.Join(',', values) : null);
                private static void WriteProperty(string name, bool? value)
                {
                    WriteProperty(
                        name,
                        value switch
                        {
                            true => "true",
                            false => "false",
                            null => null
                        }
                    );
                }
            }
        }
    }
}
