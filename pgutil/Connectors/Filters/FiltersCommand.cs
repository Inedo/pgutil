using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed partial class FiltersCommand : IConsoleCommandContainer
        {
            public static string Name => "filters";
            public static string Description => "List and update connector filters";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<ConnectorOption>()
                    .WithCommand<ListCommand>()
                    .WithCommand<RemoveCommand>()
                    .WithCommand<AddCommand>();
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
