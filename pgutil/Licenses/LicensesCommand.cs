using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand : IConsoleCommandContainer
    {
        public static string Name => "licenses";
        public static string Description => "Manage license definitions and audit package licenses";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithCommand<CreateCommand>()
                .WithCommand<InfoCommand>()
                .WithCommand<ListCommand>()
                .WithCommand<FilesCommand>()
                .WithCommand<DetectionCommand>()
                .WithCommand<DeleteCommand>();
        }

        private sealed class CodeOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--code";
            public static string Description => "Unique ID of the license";
        }
    }
}
