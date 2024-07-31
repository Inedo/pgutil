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
                .WithCommand<ListCommand>()
                .WithCommand<CreateCommand>()
                .WithCommand<DeleteCommand>()
                .WithCommand<AddFileCommand>()
                .WithCommand<RemoveFileCommand>();
        }
    }
}
