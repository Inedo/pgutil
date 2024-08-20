using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed partial class RetentionCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "Lists retention rules on a feed";
                public static string Examples => """
                      $> pgutil feeds retention list --feed=public-nuget

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-update
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = await client.GetFeedAsync(context.GetFeedName(), cancellationToken);
                    if (feed.RetentionRules is null || feed.RetentionRules.Length == 0)
                    {
                        CM.WriteLine(ConsoleColor.DarkGray, "* no rules defined *");
                        return 0;
                    }

                    int index = 1;
                    foreach (var rule in feed.RetentionRules)
                    {
                        CM.WriteLine($"[{index++}]");
                        WriteProperty("deleteCached", rule.DeleteCached);
                        WriteProperty("deletePackageIds", rule.DeletePackageIds);
                        WriteProperty("deletePrereleaseVersions", rule.DeletePrereleaseVersions);
                        WriteProperty("deleteVersions", rule.DeleteVersions);
                        WriteProperty("keepConsumedWithinDays", rule.KeepConsumedWithinDays?.ToString());
                        WriteProperty("keepIfActivelyConsumed", rule.KeepIfActivelyConsumed);
                        WriteProperty("keepPackageIds", rule.KeepPackageIds);
                        WriteProperty("keepPackageUsageRemovedDays", rule.KeepPackageUsageRemovedDays?.ToString());
                        WriteProperty("keepUsedWithinDays", rule.KeepUsedWithinDays?.ToString());
                        WriteProperty("keepVersionsCount", rule.KeepVersionsCount?.ToString());
                        WriteProperty("sizeExclusive", rule.SizeExclusive);
                        WriteProperty("sizeTrigger", rule.SizeTriggerKb?.ToString());
                        WriteProperty("triggerDownloadCount", rule.TriggerDownloadCount?.ToString());
                    }

                    return 0;
                }

                private static void WriteProperty(string name, string? value)
                {
                    CM.Write(" ", new TextSpan(name, ConsoleColor.White), new TextSpan("=", ConsoleColor.Yellow));
                    if (value is null)
                        CM.Write(ConsoleColor.DarkGray, "*not set*");
                    else
                        CM.Write(ConsoleColor.Blue, value);

                    Console.WriteLine();
                }
                private static void WriteProperty(string name, string[]? values) => WriteProperty(name, values?.Length > 0 ? string.Join(',', values) : null);
                private static void WriteProperty(string name, bool? value)
                {
                    WriteProperty(
                        name,
                        value switch
                        {
                            true => "true",
                            false => "false",
                            null => null
                        }
                    );
                }
            }
        }
    }
}
