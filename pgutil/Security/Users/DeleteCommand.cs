using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class UsersCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a user account";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<UserNameOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var user = context.GetOption<UserNameOption>();

                    CM.WriteLine($"Deleting user ", new TextSpan(user, ConsoleColor.White), "...");
                    await client.DeleteUserAsync(user, cancellationToken);

                    CM.WriteLine("User deleted.");
                    return 0;
                }
            }
        }
    }
}
