using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class CommentsCommand : IConsoleCommandContainer
        {
            public static string Name => "comments";
            public static string Description => "Views and manages build comments";

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
                    .WithCommand<CreateCommand>();
            }

            private sealed class ProjectOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--project";
                public static string Description => "Name of the project";
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
                public static string Description => "Comment number";
            }
        }
    }
}
