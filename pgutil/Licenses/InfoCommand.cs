using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed class InfoCommand : IConsoleCommand
        {
            public static string Name => "info";
            public static string Description => "Displays information about a license";
            public static string Examples => """
                  $> pgutil licenses info --code=MIT

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-licenses/proget-api-licenses-get
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<CodeOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var code = context.GetOption<CodeOption>();
                var license = await client.ListLicensesAsync(cancellationToken).FirstOrDefaultAsync(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                if (license is null)
                {
                    CM.WriteError($"License {code} not found.");
                    return -1;
                }

                Console.WriteLine($"Code: {license.Code}");
                Console.WriteLine($"Title: {license.Title}");
                Console.WriteLine();
                Console.WriteLine("Detection:");
                if (license.Spdx is not null)
                    Console.WriteLine($" SPDX: {string.Join(", ", license.Spdx)}");
                
                if (license.Urls is not null)
                {
                    Console.WriteLine(" Url:");
                    foreach (var url in license.Urls)
                        Console.WriteLine($"  - {url}");
                }

                if (license.PackageNames is not null)
                {
                    Console.Write(" Packages:");
                    foreach (var p in license.PackageNames)
                        Console.WriteLine($"  - {p}");
                }

                if (license.Purls is not null)
                {
                    Console.Write(" PUrls:");
                    foreach (var p in license.Purls)
                        Console.WriteLine($"  - {p}");
                }

                if (license.Hashes is not null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Files:");
                    foreach (var f in license.Hashes)
                        Console.WriteLine($" - {f}");
                }

                return 0;
            }
        }
    }
}
