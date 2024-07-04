using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed partial class FiltersCommand
        {
            private sealed class RemoveCommand : IConsoleCommand
            {
                public static string Name => "remove";
                public static string Description => "Removes a connector filter";

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

                    int index = Array.IndexOf(filters, filter);
                    if (index < 0)
                    {
                        CM.WriteError<FilterOption>($"{filter} is not a filter on this connector");
                        return -1;
                    }

                    connector.Filters = [.. filters.AsSpan(0, index), .. filters.AsSpan(index + 1)];

                    _ = await client.UpdateConnectorAsync(connector.Name, connector, cancellationToken);

                    Console.WriteLine("Filter removed.");

                    return 0;
                }

                private sealed class FilterOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--filter";
                    public static string Description => "The filter to remove";
                }
            }
        }
    }
}
