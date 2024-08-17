using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SettingsCommand
    {
        private sealed class SetCommand : IConsoleCommand
        {
            public static string Name => "set";
            public static string Description => "Sets the value of setting in ProGet";
            public static string Examples => """
                  $> pgutil settings set --name=Diagnostics.MinimumLogLevel --value=30

                  $> pgutil settings set --name=Retention.KeepLatestExecutionCount --value=10

                  $> pgutil settings set --name=Service.FeedReplicationExecuterThrottle --value=30
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder
                    .WithOption<NameOption>()
                    .WithOption<ValueOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var name = context.GetOption<NameOption>();
                var value = context.GetOptionOrDefault<ValueOption>();

                await client.SetSettingAsync(name, value, cancellationToken);

                return 0;
            }

            private sealed class NameOption : IConsoleOption
            {
                public static string Name => "--name";
                public static string Description => "Name of the setting value to change";
                public static bool Required => true;
            }

            private sealed class ValueOption : IConsoleOption
            {
                public static string Name => "--value";
                public static string Description => "New value of the setting";
                public static bool Required => false;
            }
        }
    }
}
