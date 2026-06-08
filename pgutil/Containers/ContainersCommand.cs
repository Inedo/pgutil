using ConsoleMan;

namespace PgUtil;

partial class Program
{
    private sealed partial class ContainersCommand : IConsoleCommandContainer
    {
        public static string Name => "containers";
        public static string Description => "Perform operations against a container feed in ProGet";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithProGetClientOptions()
                .WithOption<FeedOption>()
                .WithCommand<AuditCommand>()
                .WithCommand<TagsCommand>()
                .WithCommand<ImagesCommand>();
        }
    }
}