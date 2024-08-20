using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand : IConsoleCommandContainer
    {
        public static string Name => "apikeys";
        public static string Description => "Manages ProGet API keys";
        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithCommand<ListCommand>()
                .WithCommand<DeleteCommand>()
                .WithCommand<CreateCommand>();
        }
    }
}
