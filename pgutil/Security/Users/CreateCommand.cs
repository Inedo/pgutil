using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class UsersCommand
        {
            private sealed class CreateCommand : IConsoleCommand
            {
                public static string Name => "create";
                public static string Description => "Creates a new user account";
                public static string Examples => """
                      $> pgutil security users create --username="John Smith" --displayname=jsmith --email=jsmith@kramerica.com  --password=password123

                    For more information, see: https://docs.inedo.com/docs/proget/api/security/users/create
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<UserNameOption>()
                        .WithOption<DisplayNameOption>()
                        .WithOption<EmailOption>(true)
                        .WithOption<PasswordOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var userName = context.GetOption<UserNameOption>();
                    var displayName = context.GetOption<DisplayNameOption>();
                    var email = context.GetOption<EmailOption>();

                    CM.WriteLine("Creating user ", new TextSpan($"{userName} ({displayName})", ConsoleColor.White), "...");

                    if (!context.TryGetOption<PasswordOption>(out var password))
                    {
                        if (!Console.IsInputRedirected && !Console.IsOutputRedirected)
                        {
                            Console.Write("Enter password for user: ");
                            password = Console.ReadLine();
                            if (string.IsNullOrEmpty(password))
                            {
                                CM.WriteError("Password is required.");
                                return -1;
                            }

                            Console.Write("Confirm password: ");
                            if (password != Console.ReadLine())
                            {
                                CM.WriteError("Passwords did not match.");
                                return -1;
                            }
                        }
                        else if (Console.IsInputRedirected)
                        {
                            password = Console.In.ReadLine();
                        }
                    }

                    if (string.IsNullOrEmpty(password))
                    {
                        CM.WriteError("Password must be supplied either using the --password option or direct input.");
                        return -1;
                    }

                    await client.CreateUserAsync(
                        new SecurityUser
                        {
                            Name = userName,
                            DisplayName = displayName,
                            Email = email,
                            Password = password
                        },
                        cancellationToken
                    );

                    CM.WriteLine("User created.");
                    return 0;
                }
            }
        }
    }
}
