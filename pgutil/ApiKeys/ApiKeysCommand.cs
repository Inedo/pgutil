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
            builder.WithProGetClientOptions()
                .WithCommand<CreateCommand>()
                .WithCommand<ListCommand>()
                .WithCommand<DeleteCommand>();
        }
    }
}
