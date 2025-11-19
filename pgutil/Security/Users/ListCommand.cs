using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class UsersCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "Lists user accounts";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<SearchTermOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var searchTerm = context.GetOptionOrDefault<SearchTermOption>();

                    bool any = false;

                    await foreach (var user in client.ListUsersAsync(cancellationToken))
                    {
                        if (string.IsNullOrEmpty(searchTerm) || isInclused(searchTerm, user))
                        {
                            any = true;
                            CM.WriteLine($"{user.Name} ({user.DisplayName})");
                        }
                    }

                    if (!any)
                        CM.WriteLine(ConsoleColor.Yellow, "No matching users found.");

                    return 0;

                    static bool isInclused(string searchTerm, SecurityUser user)
                    {
                        return user.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                            || (user.DisplayName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).GetValueOrDefault();
                    }
                }

                private sealed class SearchTermOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--search-term";
                    public static string Description => "Searches for users based on this value";
                }
            }
        }
    }
}
