namespace ConsoleMan;

public interface IConsoleCommand : IConsoleArgument
{
    static abstract Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken);
    static abstract void Configure(ICommandBuilder builder);
}
