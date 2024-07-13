using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class ProjectsCommand : IConsoleCommandContainer
        {
            public static string Name => "projects";
            public static string Description => "View and manage projects";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithCommand<ListCommand>()
                    .WithCommand<CreateCommand>();
            }
        }
    }
}
