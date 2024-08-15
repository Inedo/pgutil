using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class RetentionCommand
        {
            private sealed class UpdateCommand : IConsoleCommand
            {
                public static string Name => "update";
                public static string Description => "Updates an existing retention rule";
                public static string Examples => """
                      $> pgutil feeds retention update --feed=public-nuget --deletePrereleaseVersions=true --rule=1

                      $> pgutil feeds retention update --feed=approved-npm --keepUsedWithinDays= --rule=3

                      $> pgutil feeds retention update --feed=public-pypi --deleteCached=true --keepVersionsCount=5 --rule=5

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-update
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<RuleOption>();
                    RetentionOptions.Configure(builder);
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    int rule = context.GetOption<RuleOption, int>();
                    if (rule < 1)
                    {
                        CM.WriteError<RuleOption>("value must be 1 or higher");
                        return -1;
                    }

                    var client = context.GetProGetClient();
                    var feed = await client.GetFeedAsync(context.GetFeedName(), cancellationToken);

                    if (feed.RetentionRules is null || feed.RetentionRules.Length < rule)
                    {
                        CM.WriteError<RuleOption>($"feed does not have a retention rule #{rule}");
                        return -1;
                    }

                    RetentionOptions.Assign(context, feed.RetentionRules[rule - 1]);

                    _ = await client.UpdateFeedAsync(context.GetFeedName(), feed, cancellationToken);

                    Console.WriteLine($"Rule #{feed.RetentionRules.Length + 1} updated.");
                    return 0;
                }

                private sealed class RuleOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--rule";
                    public static string Description => "Rule number to update";
                }
            }
        }
    }
}
