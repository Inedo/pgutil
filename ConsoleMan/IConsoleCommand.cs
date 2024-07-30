namespace ConsoleMan;

public interface IConsoleCommand : IConsoleArgument
{
    static virtual bool AllowAdditionalOptions => false;
    static virtual string? Examples => null;

    static abstract Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken);
    static abstract void Configure(ICommandBuilder builder);
}
