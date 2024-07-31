using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class FilesCommand : IConsoleCommandContainer
        {
            public static string Name => "files";
            public static string Description => "Add, remove, or view license files";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<AddCommand>()
                    .WithCommand<DeleteCommand>()
                    .WithCommand<ShowCommand>()
                    .WithCommand<ListCommand>();
            }
        }
    }
}
