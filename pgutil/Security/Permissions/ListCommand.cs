using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class PermissionsCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "List configured permissions";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await foreach (var p in client.ListPermissionsAsync(cancellationToken).OrderBy(p => p.Id.GetValueOrDefault()))
                    {
                        CM.Write(new TextSpan(p.Id.ToString(), ConsoleColor.White), ": ");
                        FormatPermission(p);
                        Console.WriteLine();
                    }

                    return 0;
                }
            }
        }
    }
}
