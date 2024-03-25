namespace ConsoleMan;

public interface IConsoleCommandContainer : IConsoleCommand
{
    static Task<int> IConsoleCommand.ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        context.WriteUsage();
        return Task.FromResult(1);
    }
}
