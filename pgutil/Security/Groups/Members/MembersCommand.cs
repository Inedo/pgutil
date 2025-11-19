using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class GroupsCommand
        {
            private sealed partial class MembersCommand : IConsoleCommandContainer
            {
                public static string Name => "members";
                public static string Description => "Manage group membership";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithCommand<AddCommand>()
                        .WithCommand<RemoveCommand>()
                        .WithCommand<ListCommand>();
                }

                private sealed class MemberOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--member";
                    public static string Description => "Member of the group";
                }
            }
        }
    }
}
