using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class PropertiesCommand : IConsoleCommandContainer
        {
            public static string Name => "properties";
            public static string Description => "Lists or modifies values of feed properties";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<FeedOption>()
                    .WithCommand<ListCommand>()
                    .WithCommand<SetCommand>();
            }
        }
    }
}
