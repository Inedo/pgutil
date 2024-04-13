using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists licenses known by ProGet";

            public static void Configure(ICommandBuilder builder)
            {
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                await foreach (var license in client.ListLicensesAsync(cancellationToken))
                {
                    CM.WriteLine(new TextSpan(license.Code, ConsoleColor.White), $": {license.Title}");
                    Console.WriteLine();
                }

                return 0;
            }
        }
    }
}
