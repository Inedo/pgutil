using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand : IConsoleCommandContainer
    {
        public static string Name => "connectors";
        public static string Description => "Views and manages ProGet connectors";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithCommand<CreateCommand>()
                .WithCommand<PropertiesCommand>()
                .WithCommand<ListCommand>()
                .WithCommand<FiltersCommand>()
                .WithCommand<DeleteCommand>();
        }
    }
}
