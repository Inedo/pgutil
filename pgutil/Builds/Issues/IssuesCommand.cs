using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class IssuesCommand : IConsoleCommandContainer
        {
            public static string Name => "issues";
            public static string Description => "Views and manages build issues";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<ProjectOption>()
                    .WithOption<BuildOption>()
                    .WithCommand<ListCommand>()
                    .WithCommand<DeleteCommand>()
                    .WithCommand<ResolveCommand>();
            }

            private sealed class BuildOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--build";
                public static string Description => "Project build number";
            }

            private sealed class NumberOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--number";
                public static string Description => "Issue number";
            }
        }
    }
}
