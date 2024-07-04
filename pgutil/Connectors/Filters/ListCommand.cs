using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed partial class FiltersCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "List connector filters";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var connector = await client.GetConnectorAsync(context.GetOption<ConnectorOption>(), cancellationToken);

                    if (connector.Filters is null || connector.Filters.Length == 0)
                    {
                        CM.WriteLine(ConsoleColor.DarkGray, "* no filters defined *");
                        return 0;
                    }

                    foreach (var item in connector.Filters)
                        Console.WriteLine(item);

                    return 0;
                }
            }
        }
    }
}
