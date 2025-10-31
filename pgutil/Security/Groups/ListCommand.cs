using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class GroupsCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "Lists user groups";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    bool any = false;
                    await foreach (var group in client.ListUserGroups(cancellationToken))
                    {
                        any = true;
                        CM.WriteLine(new TextSpan(group.Name, ConsoleColor.White), $" (members: {group.Users?.Length})");
                    }

                    if (!any)
                        CM.WriteLine("No groups defined.");

                    return 0;
                }
            }
        }
    }
}
