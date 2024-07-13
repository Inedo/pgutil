namespace ConsoleMan;

public class ConsoleManException : Exception
{
    public ConsoleManException(string? message = null, int exitCode = -1) : base(message)
    {
        this.ExitCode = exitCode;
        this.HasMessage = message is not null;
    }

    public int ExitCode { get; }
    public bool HasMessage { get; }
}
