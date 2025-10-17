using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class ProjectsCommand : IConsoleCommandContainer
        {
            public static string Name => "projects";
            public static string Description => "Views and manages projects";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithProGetClientOptions()
                    .WithCommand<ListCommand>()
                    .WithCommand<CreateCommand>();
            }
        }
    }
}
