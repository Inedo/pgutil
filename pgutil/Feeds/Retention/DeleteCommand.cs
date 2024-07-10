using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class RetentionCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a retention rule from a feed";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<RuleOption>();
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

                    feed.RetentionRules = [.. feed.RetentionRules.AsSpan(0, rule - 1), .. feed.RetentionRules.AsSpan(rule)];

                    _ = await client.UpdateFeedAsync(context.GetFeedName(), feed, cancellationToken);

                    Console.WriteLine("Rule deleted.");

                    return 0;
                }

                private sealed class RuleOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--rule";
                    public static string Description => "Rule number to delete";
                }
            }
        }
    }
}
