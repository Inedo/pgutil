using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed class AddFileCommand : IConsoleCommand
        {
            public static string Name => "addfile";
            public static string Description => "Uploads a license file to ProGet";

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

            private sealed class CodeOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--code";
                public static string Description => "Unique ID of the license";
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
