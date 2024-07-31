using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed class CreateCommand : IConsoleCommand
        {
            public static string Name => "create";
            public static string Description => "Adds a license definition to ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<CodeOption>()
                    .WithOption<TitleOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var license = new LicenseInfo
                {
                    Code = context.GetOption<CodeOption>(),
                    Title = context.GetOption<TitleOption>(),
                    Spdx = context.GetOptionOrDefault<SpdxOption>()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                };

                CM.WriteLine("Adding ", new TextSpan($"{license.Code}: {license.Title}", ConsoleColor.White));
                await client.AddLicenseAsync(license, cancellationToken);
                Console.WriteLine("License added.");

                return 0;
            }

            private sealed class TitleOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--title";
                public static string Description => "Friendly name of the license";
            }

            private sealed class SpdxOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--spdx";
                public static string Description => "Comma-separated list of SPDX identifiers for the license";
            }
        }
    }
}
