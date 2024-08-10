using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class FilesCommand
        {
            private sealed class AddCommand : IConsoleCommand
            {
                public static string Name => "add";
                public static string Description => "Uploads a license file to ProGet";
                public static string Examples => """
                      $> pgutil licenses files add --code=ABC-1.0 --file=C:\documents\license-files\abc-1.0-license-file.txt

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-licenses/proget-api-licenses-update
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<CodeOption>()
                        .WithOption<FileOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var code = context.GetOption<CodeOption>();
                    var licenseFileName = context.GetOption<FileOption>();

                    if (!File.Exists(licenseFileName))
                    {
                        CM.WriteError<FileOption>($"{licenseFileName} not found.");
                        return -1;
                    }

                    CM.WriteLine("Uploading ", new TextSpan(licenseFileName, ConsoleColor.White), " as ", new TextSpan(code, ConsoleColor.White), " license...");
                    using var licenseFileStream = File.OpenRead(licenseFileName);
                    await client.AddLicenseFileAsync(code, licenseFileStream, cancellationToken);
                    Console.WriteLine("License file added.");
                    return 0;
                }

                private sealed class FileOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--file";
                    public static string Description => "License file to upload to ProGet";
                }
            }
        }
    }
}
