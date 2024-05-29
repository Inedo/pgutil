namespace ConsoleMan;

public interface ICommandBuilder
{
    ICommandBuilder WithOption<TOption>(OptionOverrides? overrides) where TOption : IConsoleOption;
    ICommandBuilder WithCommand<TCommand>() where TCommand : IConsoleCommand;
}
