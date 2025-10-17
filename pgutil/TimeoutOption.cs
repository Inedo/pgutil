using ConsoleMan;

namespace PgUtil;

internal sealed class TimeoutOption : IConsoleOption
{
    public static bool Required => false;
    public static string Name => "--timeout";
    public static string Description => "Timeout period for requests to ProGet in seconds";
}
