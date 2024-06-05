using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class SettingsCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Displays the settings displayed generally shown under Admin > Advanced Settings in ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder
                    .WithOption<ExpandedOption>()
                    .WithOption<ShowAllOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var showAll = context.HasFlag<ShowAllOption>();
                var compact = !context.HasFlag<ExpandedOption>();

                await foreach (var setting in client.ListSettingsAsync(showAll, cancellationToken))
                {
                    if (compact)
                    {
                        CM.WriteLine($"{setting.Name}={setting.Value}");
                    }
                    else
                    {
                        CM.WriteLine($"{setting.Name}");
                        CM.WriteTwoColumnList(
                            ("  Type:", setting.ValueType.ToString()),
                            ("  Desc:", setting.Description ?? "(not set)"),
                            ("  Value:", setting.Value ?? "(not set)")
                        );
                        CM.WriteLine();
                    }
                }

                return 0;
            }

            internal sealed class ShowAllOption : IConsoleFlagOption
            {
                public static string Name => "--show-all";
                public static string Description => "Includes hidden settings";
                public static bool Undisclosed => true;
            }
            internal sealed class ExpandedOption : IConsoleFlagOption
            {
                public static string Name => "--expanded";
                public static string Description => "Include detail and type information about settings";
            }
        }
    }
}
