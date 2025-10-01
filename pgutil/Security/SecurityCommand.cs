using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand : IConsoleCommandContainer
    {
        public static string Name => "security";
        public static string Description => "Manage security permissions, users, and groups on ProGet";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithCommand<PermissionsCommand>();
        }
    }
}
