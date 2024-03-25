namespace PgUtil;

internal sealed class PgUtilException(string? message = null) : Exception(message)
{
    public bool HasMessage { get; } = message is not null;
}
