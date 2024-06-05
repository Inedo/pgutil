using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed partial class FolderCommand : IConsoleCommandContainer
        {
            public static string Name => "folder";
            public static string Description => "Manage asset directory folders";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<CreateCommand>()
                    .WithCommand<ImportCommand>()
                    .WithCommand<ExportCommand>();
            }
        }
    }
}
