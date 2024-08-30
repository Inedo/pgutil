using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists connectors in ProGet";

            public static void Configure(ICommandBuilder builder)
            {
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                await foreach (var connector in client.ListConnectorsAsync(cancellationToken))
                    Console.WriteLine($"{connector.Name} ({connector.FeedType}) {connector.Url}");

                return 0;
            }
        }
    }
}
