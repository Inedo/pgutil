using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class UsersCommand
        {
            private sealed class ShowCommand : IConsoleCommand
            {
                public static string Name => "show";
                public static string Description => "Displays information about a user account";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<UserNameOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var name = context.GetOption<UserNameOption>();

                    bool found = false;

                    await foreach (var user in client.ListUsersAsync(cancellationToken))
                    {
                        if (name.Equals(user.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            found = true;
                            CM.WriteLine(new TextSpan("User name: ", ConsoleColor.White), user.Name);
                            CM.WriteLine(new TextSpan("Display name: ", ConsoleColor.White), user.DisplayName);
                            CM.WriteLine(new TextSpan("Email: ", ConsoleColor.White), user.Email);
                            CM.WriteLine(ConsoleColor.White, "Group membership:");
                            if (user.Groups?.Length > 0)
                            {
                                foreach (var group in user.Groups)
                                    CM.WriteLine($" - {group}");
                            }
                            else
                            {
                                CM.WriteLine(ConsoleColor.DarkGray, " - none");
                            }

                            break;
                        }
                    }

                    if (!found)
                    {
                        CM.WriteError($"User {name} not found.");
                        return -1;
                    }

                    return 0;
                }
            }
        }
    }
}
