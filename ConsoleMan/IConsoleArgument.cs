namespace ConsoleMan;

public interface IConsoleArgument
{
    static abstract string Name { get; }
    static abstract string Description { get; }
}
