using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class FilesCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Removes a license file from ProGet";
                public static string Examples => """
                      $> pgutil licenses files delete --hash=00462de3d7b6f3e5551a69ae84344bc69d23c02e1353be3e8445d16f025e523b

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-licenses/proget-api-licenses-update
                    """;

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
}
