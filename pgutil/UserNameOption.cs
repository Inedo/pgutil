using ConsoleMan;

namespace PgUtil;

internal sealed class UserNameOption : IConsoleOption
{
    public static bool Required => false;
    public static string Name => "--username";
    public static string Description => "ProGet user name used to authorize access";
}
