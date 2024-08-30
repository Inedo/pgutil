using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand : IConsoleCommandContainer
    {
        public static string Name => "feeds";
        public static string Description => "Views and manages ProGet feeds";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithCommand<CreateCommand>()
                .WithCommand<PropertiesCommand>()
                .WithCommand<RetentionCommand>()
                .WithCommand<DeleteCommand>()
                .WithCommand<ListCommand>()
                .WithCommand<StorageCommand>();
        }
    }
}
