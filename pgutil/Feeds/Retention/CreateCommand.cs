using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class RetentionCommand
        {
            private sealed class CreateCommand : IConsoleCommand
            {
                public static string Name => "create";
                public static string Description => "Creates a new retention rule";
                public static string Examples => """
                      $> pgutil feeds retention create --feed=public-nuget --deletePrereleaseVersions=true

                      $> pgutil feeds retention update --feed=approved-npm --keepUsedWithinDays=30

                      $> pgutil feeds retention update --feed=public-pypi --deleteCached=true --keepVersionsCount=5

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-update
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    RetentionOptions.Configure(builder);
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = await client.GetFeedAsync(context.GetFeedName(), cancellationToken);

                    var rule = new RetentionRule();
                    RetentionOptions.Assign(context, rule);

                    var current = feed.RetentionRules ?? [];
                    feed.RetentionRules = [.. current, rule];

                    _ = await client.UpdateFeedAsync(context.GetFeedName(), feed, cancellationToken);

                    Console.WriteLine($"Rule #{feed.RetentionRules.Length} created.");
                    return 0;
                }
            }
        }
    }
}
