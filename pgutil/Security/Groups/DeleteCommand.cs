using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class GroupsCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a user group";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NameOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var name = context.GetOption<NameOption>();

                    CM.WriteLine("Deleting group ", new TextSpan(name, ConsoleColor.White), "...");
                    await client.DeleteUserGroupAsync(name, cancellationToken);

                    CM.WriteLine("Group deleted.");
                    return 0;
                }
            }
        }
    }
}
