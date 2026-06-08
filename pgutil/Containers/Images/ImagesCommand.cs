using ConsoleMan;

namespace PgUtil;

partial class Program
{
    partial class ContainersCommand
    {
        private sealed partial class ImagesCommand : IConsoleCommandContainer
        {
            public static string Name => "images";
            public static string Description => "Work with container images in a ProGet feed";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<DeleteCommand>();
            }
        }
    }
}
