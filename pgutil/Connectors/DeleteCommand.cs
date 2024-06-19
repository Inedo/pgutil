﻿using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed class DeleteCommand : IConsoleCommand
        {
            public static string Name => "delete";
            public static string Description => "Deletes a connector";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<NameOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var name = context.GetOption<NameOption>();

                CM.WriteLine("Deleting ", new TextSpan(name, ConsoleColor.White), "...");
                await client.DeleteConnectorAsync(name, cancellationToken);
                Console.WriteLine("Connector deleted.");
                return 0;
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the connector to delete";
            }
        }
    }
}
