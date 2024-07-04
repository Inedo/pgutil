using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed partial class PropertiesCommand : IConsoleCommandContainer
        {
            public static string Name => "properties";
            public static string Description => "List and update connector properties";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<ConnectorOption>()
                    .WithCommand<ListCommand>()
                    .WithCommand<SetCommand>();
            }

            private sealed class ConnectorOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--connector";
                public static string Description => "Name of the connector";
            }
        }
    }
}
