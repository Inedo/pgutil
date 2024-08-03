using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed class DeleteCommand : IConsoleCommand
        {
            public static string Name => "delete";
            public static string Description => "Deletes a license defintion from ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<CodeOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var code = context.GetOption<CodeOption>();
                CM.WriteLine("Deleting ", new TextSpan(code, ConsoleColor.White));

                var license = await client.ListLicensesAsync(cancellationToken).FirstOrDefaultAsync(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (license is null)
                {
                    CM.WriteError($"License {code} not found.");
                    return -1;
                }

                await client.DeleteLicenseAsync(license.Id.GetValueOrDefault(), cancellationToken);
                Console.WriteLine("License deleted.");
                return 0;
            }
        }
    }
}
