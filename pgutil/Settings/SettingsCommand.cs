using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SettingsCommand : IConsoleCommandContainer
    {
        public static string Name => "settings";
        public static string Description => "Manages ProGet Settings";
        public static void Configure(ICommandBuilder builder)
        {
            builder.WithProGetClientOptions()
                .WithCommand<ListCommand>()
                .WithCommand<SetCommand>();
        }
    }
}
