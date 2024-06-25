namespace Inedo.ProGet;

public abstract class ProGetClientException : Exception
{
    private protected ProGetClientException()
    {
    }
    private protected ProGetClientException(string? message) : base(message)
    {
    }
}
