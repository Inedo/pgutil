using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed partial class FiltersCommand
        {
            private sealed class AddCommand : IConsoleCommand
            {
                public static string Name => "add";
                public static string Description => "Adds a connector filter";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<FilterOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var connector = await client.GetConnectorAsync(context.GetOption<ConnectorOption>(), cancellationToken);

                    var filters = connector.Filters ?? [];

                    var filter = context.GetOption<FilterOption>();

                    connector.Filters = [.. filters, filter];

                    _ = await client.UpdateConnectorAsync(connector.Name, connector, cancellationToken);

                    Console.WriteLine("Filter added.");

                    return 0;
                }

                private sealed class FilterOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--filter";
                    public static string Description => "The filter to add";
                }
            }
        }
    }
}
