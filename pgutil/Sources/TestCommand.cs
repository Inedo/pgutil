using ConsoleMan;
using PgUtil.Config;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SourcesCommand
    {
        private sealed class TestCommand : IConsoleCommand
        {
            public static string Name => "test";
            public static string Description => "Test the connection to a source";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<NameOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var name = context.GetOption<NameOption>();

                var source = PgUtilConfig.Instance.Sources.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (source is null)
                {
                    CM.WriteError<NameOption>($"Source {name} not found.");
                    return -1;
                }

                var client = source.GetProGetClient();
                var info = await client.GetInstanceHealthAsync(cancellationToken);
                CM.WriteLine(ConsoleColor.Green, $"Successfully contacted ProGet {info.ReleaseNumber}.");
                return 0;
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the source.";
                public static string DefaultValue => "Default";
            }
        }
    }
}
