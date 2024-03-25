using ConsoleMan;

namespace PgUtil;

internal sealed class SourceOption : IConsoleOption
{
    public static bool Required => false;
    public static string Name => "--source";
    public static string Description => "Named source or URL of ProGet";
}
