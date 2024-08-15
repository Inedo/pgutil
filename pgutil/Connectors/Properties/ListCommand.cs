using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed partial class PropertiesCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "List the properties of a connector";
                public static string Examples => """
                      $> pgutil connectors properties list --connector=nuget.org

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-connectors/proget-api-connectors-get
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var connector = await client.GetConnectorAsync(context.GetOption<ConnectorOption>(), cancellationToken);

                    WriteProperty("url", connector.Url);
                    WriteProperty("feedType", connector.FeedType);
                    WriteProperty("timeout", connector.Timeout?.ToString());
                    WriteProperty("metadataCacheEnabled", connector.MetadataCacheEnabled);
                    WriteProperty("metadataCacheCount", connector.MetadataCacheCount?.ToString());
                    WriteProperty("metadataCacheMinutes", connector.MetadataCacheMinutes?.ToString());

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
