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
                await client.DeleteLicenseAsync(code, cancellationToken);
                Console.WriteLine("License deleted.");
                return 0;
            }

            private sealed class CodeOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--code";
                public static string Description => "Unique ID of the license";
            }
        }
    }
}
