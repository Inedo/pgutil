using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand : IConsoleCommandContainer
    {
        public static string Name => "connectors";
        public static string Description => "View and manage ProGet connectors";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithCommand<CreateCommand>()
                .WithCommand<DeleteCommand>()
                .WithCommand<ListCommand>();
        }
    }
}
