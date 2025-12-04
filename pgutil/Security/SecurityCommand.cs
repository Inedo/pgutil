using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand : IConsoleCommandContainer
    {
        public static string Name => "security";
        public static string Description => "Manages security permissions, users, and groups on ProGet";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithProGetClientOptions()
                .WithCommand<PermissionsCommand>()
                .WithCommand<TasksCommand>()
                .WithCommand<UsersCommand>()
                .WithCommand<GroupsCommand>();
        }
    }
}
