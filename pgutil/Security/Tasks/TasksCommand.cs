using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class TasksCommand : IConsoleCommandContainer
        {
            public static string Name => "tasks";
            public static string Description => "Manage security tasks on ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<ListCommand>()
                    .WithCommand<CreateCommand>()
                    .WithCommand<DeleteCommand>();
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the security task";
            }

            private sealed class DescriptionOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--description";
                public static string Description => "Friendly description of the security task";
            }

            private sealed class AttributesOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--attributes";
                public static string Description => "Comma separated security attributes to assign to the task";
            }
        }
    }
}
