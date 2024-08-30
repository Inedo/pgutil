using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class FilesCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "Lists license files known by ProGet";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    await foreach (var license in client.ListLicensesAsync(cancellationToken))
                    {
                        if (license.Hashes?.Count > 0)
                            Console.WriteLine($"{license.Code} {string.Join(", ", license.Hashes)}");
                    }

                    return 0;
                }
            }
        }
    }
}
