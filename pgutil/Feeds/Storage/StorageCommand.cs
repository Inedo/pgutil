using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class StorageCommand : IConsoleCommandContainer
        {
            public static string Name => "storage";
            public static string Description => "View and configure feed storage options";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<TypesCommand>()
                    .WithCommand<InfoCommand>()
                    .WithCommand<ChangeCommand>();
            }
        }
    }
}
