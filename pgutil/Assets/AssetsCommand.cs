using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand : IConsoleCommandContainer
    {
        public static string Name => "assets";
        public static string Description => "Works with a ProGet asset directory";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<SourceOption>()
                .WithOption<ApiKeyOption>()
                .WithOption<UserNameOption>()
                .WithOption<PasswordOption>()
                .WithOption<FeedOption>()
                .WithCommand<ListCommand>()
                .WithCommand<DownloadCommand>()
                .WithCommand<UploadCommand>()
                .WithCommand<DeleteCommand>()
                .WithCommand<FolderCommand>()
                .WithCommand<MetadataCommand>();
        }
    }
}
