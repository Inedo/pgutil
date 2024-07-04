using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed partial class PropertiesCommand
        {
            private sealed class SetCommand : IConsoleCommand
            {
                public static string Name => "set";
                public static string Description => "Updates a connector property value";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<PropertyOption>()
                        .WithOption<ValueOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var connector = await client.GetConnectorAsync(context.GetOption<ConnectorOption>(), cancellationToken);

                    var name = context.GetOption<PropertyOption>();
                    var value = context.GetOption<ValueOption>();
                    switch (name)
                    {
                        case "timeout":
                            connector.Timeout = int.Parse(value);
                            break;

                        case "metadataCacheEnabled":
                            connector.MetadataCacheEnabled = getBool(value);
                            break;

                        case "metadataCacheCount":
                            connector.MetadataCacheCount = int.Parse(value);
                            break;

                        case "metadataCacheMinutes":
                            connector.MetadataCacheMinutes = int.Parse(value);
                            break;

                        default:
                            CM.WriteError<PropertyOption>($"{name} is read only");
                            return -1;
                    }

                    _ = await client.UpdateConnectorAsync(connector.Name, connector, cancellationToken);

                    Console.WriteLine("Property updated.");

                    return 0;

                    static bool getBool(string value)
                    {
                        if (bool.TryParse(value, out var b))
                            return b;

                        CM.WriteError<ValueOption>("must be true or false for this property");
                        throw new PgUtilException();
                    }
                }

                private sealed class PropertyOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--property";
                    public static string Description => "Name of connector property to set";
                    public static string[] ValidValues => ["timeout", "metadataCacheEnabled", "metadataCacheCount", "metadataCacheMinutes"];
                }

                private sealed class ValueOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--value";
                    public static string Description => "New value to assign to connector property";
                }
            }
        }
    }
}
