using ConsoleMan;

namespace PgUtil;

internal sealed class PasswordOption : IConsoleOption
{
    public static bool Required => false;
    public static string Name => "--password";
    public static string Description => "ProGet user password used to authorize access";
}
