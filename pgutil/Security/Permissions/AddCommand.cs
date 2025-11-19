using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class PermissionsCommand
        {
            private sealed class AddCommand : IConsoleCommand
            {
                public static string Name => "add";
                public static string Description => "Adds a permission";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<TaskOption>()
                        .WithOption<UserOption>()
                        .WithOption<GroupOption>()
                        .WithOption<FeedOption>()
                        .WithOption<FeedGroupOption>()
                        .WithOption<DenyFlag>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var permission = new SecurityPermission
                    {
                        Task = context.GetOption<TaskOption>(),
                        User = context.GetOptionOrDefault<UserOption>(),
                        Group = context.GetOptionOrDefault<GroupOption>(),
                        Feed = context.GetOptionOrDefault<FeedOption>(),
                        FeedGroup = context.GetOptionOrDefault<FeedGroupOption>(),
                        Deny = context.HasFlag<DenyFlag>()
                    };

                    if (!string.IsNullOrEmpty(permission.Feed) && !string.IsNullOrEmpty(permission.FeedGroup))
                    {
                        CM.WriteError("Cannot specify both --feed and --feed-group");
                        return -1;
                    }

                    if (string.IsNullOrEmpty(permission.User) && string.IsNullOrEmpty(permission.Group))
                    {
                        CM.WriteError("Either --user or --group must be specified");
                        return -1;
                    }

                    if (!string.IsNullOrEmpty(permission.User) && !string.IsNullOrEmpty(permission.Group))
                    {
                        CM.WriteError("Cannot specify both --user and --group");
                        return -1;
                    }

                    CM.Write("Adding ");
                    FormatPermission(permission);
                    CM.WriteLine("...");

                    await client.AddPermissionAsync(permission, cancellationToken);

                    CM.WriteLine("Permission added.");
                    return 0;
                }

                private sealed class TaskOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--task";
                    public static string Description => "Name of security task";
                }

                private sealed class UserOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--user";
                    public static string Description => "User name to scope permission to";
                }

                private sealed class GroupOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--group";
                    public static string Description => "User group name to scope permission to";
                }

                private sealed class FeedOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--feed";
                    public static string Description => "Feed to scope permission to";
                }

                private sealed class FeedGroupOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--feed-group";
                    public static string Description => "Feed group to scope permission to";
                }

                private sealed class DenyFlag : IConsoleFlagOption
                {
                    public static string Name => "--deny";
                    public static string Description => "Create this as a deny permission instead of a grant";
                }
            }
        }
    }
}
