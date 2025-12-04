using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class UsersCommand
        {
            private sealed class EditCommand : IConsoleCommand
            {
                public static string Name => "edit";
                public static string Description => "Modifies an existing user account";
                public static string Examples => """
                      $> pgutil security users edit --username="John Smith" --displayname=johnsmith
                      $> pgutil security users edit --username="David Jones" --password=newpassword123
                      $> pgutil security security users edit --username="Robert Davies" --displayname=rdavies --email=newemail@kramerica.com

                    For more information, see: https://docs.inedo.com/docs/proget/api/security/users/edit
                    """;
                
                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<UserNameOption>()
                        .WithOption<DisplayNameOption>(false)
                        .WithOption<EmailOption>()
                        .WithOption<PasswordOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var userName = context.GetOption<UserNameOption>();
                    var displayName = context.GetOptionOrDefault<DisplayNameOption>();
                    var email = context.GetOptionOrDefault<EmailOption>();
                    var password = context.GetOptionOrDefault<PasswordOption>();

                    CM.WriteLine("Updating user ", new TextSpan(userName, ConsoleColor.White), "...");

                    await client.UpdateUserAsync(
                        new SecurityUser
                        {
                            Name = userName,
                            DisplayName = displayName,
                            Email = email,
                            Password = password
                        },
                        cancellationToken
                    );

                    CM.WriteLine("User updated.");
                    return 0;
                }
            }
        }
    }
}
