using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class GroupsCommand : IConsoleCommandContainer
        {
            public static string Name => "groups";
            public static string Description => "Manages user groups in ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<CreateCommand>()
                    .WithCommand<DeleteCommand>()
                    .WithCommand<ListCommand>()
                    .WithCommand<MembersCommand>();
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the group";
            }
        }
    }
}
