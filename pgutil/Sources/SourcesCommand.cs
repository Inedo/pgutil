using ConsoleMan;
using PgUtil.Config;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SourcesCommand : IConsoleCommandContainer
    {
        public static string Name => "sources";
        public static string Description => $"Manages the list of sources located in the configuration file located at {PgUtilConfig.ConfigFilePath}";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithCommand<ListCommand>()
                .WithCommand<AddCommand>()
                .WithCommand<RemoveCommand>()
                .WithCommand<TestCommand>();
        }
    }
}
