using ConsoleMan;
using PgUtil.Config;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SourcesCommand
    {
        private sealed class RemoveCommand : IConsoleCommand
        {
            public static string Name => "remove";
            public static string Description => "Removes a source from the configuration file";
            public static string Examples => """
                  $> pgutil sources remove --name=Default
                  $> pgutil sources remove --name=main

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-pgutil#working-with-sources
                """;

            public static void Configure(ICommandBuilder builder) => builder.WithOption<NameOption>();

            public static Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var name = context.GetOption<NameOption>();

                var sources = PgUtilConfig.Instance.Sources.ToList();
                if (sources.RemoveAll(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == 0)
                {
                    CM.WriteLine(ConsoleColor.Yellow, $"Source {name} not found.");
                }
                else
                {
                    var inst = PgUtilConfig.Instance with { Sources = [.. sources] };
                    inst.Save(PgUtilConfig.ConfigFilePath);
                    CM.WriteLine("Removed ", new TextSpan(name, ConsoleColor.White), " source.");
                }

                return Task.FromResult(0);
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the source.";
            }
        }
    }
}
