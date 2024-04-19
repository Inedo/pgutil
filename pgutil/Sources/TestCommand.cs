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
                var sources = PgUtilConfig.Instance.Sources;

                if (context.TryGetOption<NameOption>(out var name))
                {
                    sources = sources.Where(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (sources.Length == 0)
                    {
                        CM.WriteError<NameOption>($"Source {name} not found.");
                        return -1;
                    }
                }
                if (sources.Length == 0)
                {
                    CM.WriteError("No sources were found.");
                    return -1;

                }

                var result = 0;
                foreach (var source in sources)
                {
                    var client = source.GetProGetClient();
                    try
                    {
                        var info = await client.GetInstanceHealthAsync(cancellationToken);
                        CM.WriteLine(ConsoleColor.Green, $"[{source.Name}] Successfully contacted ProGet {info.ReleaseNumber}.");
                    }
                    catch (Exception ex)
                    {
                        CM.WriteError($"[{source.Name}] Error contacted ProGet: {ex.Message}");
                        result = -1;
                    }
                }

                return result;
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--name";
                public static string Description => "Name of the source.";
            }
        }
    }
}
