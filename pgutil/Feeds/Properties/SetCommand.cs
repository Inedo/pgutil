using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class PropertiesCommand
        {
            private sealed class SetCommand : IConsoleCommand
            {
                public static string Name => "set";
                public static string Description => "Updates the value of a feed property";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<PropertyOption>()
                        .WithOption<ValueOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var feed = new ProGetFeed();

                    var name = context.GetOption<PropertyOption>();
                    var value = context.GetOption<ValueOption>();
                    switch (name)
                    {
                        case "alternateNames":
                            feed.AlternateNames = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            break;

                        case "active":
                            feed.Active = getBool(value);
                            break;

                        case "dropPath":
                            feed.DropPath = value;
                            break;

                        case "connectors":
                            feed.Connectors = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            break;

                        case "vulnerabilitiesEnabled":
                            feed.VulnerabilitiesEnabled = getBool(value);
                            break;

                        default:
                            CM.WriteError<PropertyOption>($"{name} is read only");
                            return -1;
                    }

                    _ = await client.UpdateFeedAsync(context.GetFeedName(), feed, cancellationToken);

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
                    public static string Description => "Name of feed property to set";
                    public static string[] ValidValues => ["alternateNames", "active", "dropPath", "connectors", "vulnerabilitiesEnabled"];
                }

                private sealed class ValueOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--value";
                    public static string Description => "New value to assign to feed property";
                }
            }
        }
    }
}
