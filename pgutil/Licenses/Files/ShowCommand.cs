﻿using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class FilesCommand
        {
            private sealed class ShowCommand : IConsoleCommand
            {
                public static string Name => "show";
                public static string Description => "Displays the content of a license file";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<CodeOption>(false)
                        .WithOption<HashOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var code = context.GetOptionOrDefault<CodeOption>();
                    var hash = context.GetOptionOrDefault<HashOption>();

                    string license;
                    if (!string.IsNullOrEmpty(hash))
                        license = await client.GetLicenseFileFromHashAsync(hash, cancellationToken);
                    else if (!string.IsNullOrEmpty(code))
                        license = await client.GetLicenseFileAsync(code, cancellationToken);
                    else
                        throw new PgUtilException("License file must be specified with either --code or --hash");

                    Console.WriteLine();
                    Console.WriteLine(license);
                    return 0;
                }

                private sealed class HashOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--hash";
                    public static string Description => "Hash of the license file";
                }
            }
        }
    }
}