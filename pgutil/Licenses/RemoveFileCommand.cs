using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed class RemoveFileCommand : IConsoleCommand
        {
            public static string Name => "removefile";
            public static string Description => "Removes a license file from ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<HashOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var hash = context.GetOption<HashOption>();

                CM.WriteLine("Deleting ", new TextSpan(hash, ConsoleColor.White), "...");
                await client.DeleteLicenseFileAsync(hash, cancellationToken);
                Console.WriteLine("License file deleted.");
                return 0;
            }

            private sealed class HashOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--hash";
                public static string Description => "Hash of license file to delete";
            }
        }
    }
}
