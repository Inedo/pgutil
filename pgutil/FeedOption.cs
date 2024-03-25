using ConsoleMan;

namespace PgUtil;

internal sealed class FeedOption : IConsoleOption
{
    public static bool Required => false;
    public static string Name => "--feed";
    public static string Description => "Name of feed in ProGet";
}
