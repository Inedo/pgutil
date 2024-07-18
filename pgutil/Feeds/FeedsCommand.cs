using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand : IConsoleCommandContainer
    {
        public static string Name => "feeds";
        public static string Description => "View and manage ProGet feeds";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithCommand<CreateCommand>()
                .WithCommand<DeleteCommand>()
                .WithCommand<ListCommand>()
                .WithCommand<PropertiesCommand>()
                .WithCommand<RetentionCommand>()
                .WithCommand<StorageCommand>();
        }
    }
}
