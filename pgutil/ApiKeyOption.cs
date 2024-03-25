using ConsoleMan;

namespace PgUtil;

internal sealed class ApiKeyOption : IConsoleOption
{
    public static bool Required => false;
    public static string Name => "--api-key";
    public static string Description => "ProGet API key used to authorize access";
}
