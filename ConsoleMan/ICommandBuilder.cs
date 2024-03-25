namespace ConsoleMan;

public interface ICommandBuilder
{
    ICommandBuilder WithOption<TOption>() where TOption : IConsoleOption;
    ICommandBuilder WithCommand<TCommand>() where TCommand : IConsoleCommand;
}
