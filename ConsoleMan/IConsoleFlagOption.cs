namespace ConsoleMan;

public interface IConsoleFlagOption : IConsoleOption
{
    static bool IConsoleOption.Required => false;
    static bool IConsoleOption.HasValue => false;
}
