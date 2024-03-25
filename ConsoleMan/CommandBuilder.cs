namespace ConsoleMan;

internal sealed class CommandBuilder<TContainerCommand> : ICommandBuilder where TContainerCommand : IConsoleCommand
{
    private readonly List<Command> commands = [];
    private readonly List<Option> options = [];

    public ICommandBuilder WithCommand<TCommand>() where TCommand : IConsoleCommand
    {
        var builder = new CommandBuilder<TCommand>();
        TCommand.Configure(builder);
        this.commands.Add(builder.Build());
        return this;
    }
    public ICommandBuilder WithOption<TOption>() where TOption : IConsoleOption
    {
        this.options.Add(new Option<TOption>());
        return this;
    }
    public Command<TContainerCommand> Build()
    {
        var command = new Command<TContainerCommand>(this.commands, this.options);
        foreach (var c in command.Subcommands)
            c.Parent = command;
        return command;
    }
}
