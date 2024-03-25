using System.Net;

namespace Inedo.ProGet;

public sealed class ProGetClientException : Exception
{
    public ProGetClientException(HttpStatusCode statusCode, string response)
    {
        this.StatusCode = statusCode;
        this.Response = response;
    }

    public HttpStatusCode StatusCode { get; }
    public string Response { get; }
    public override string Message => $"Server responded with {this.StatusCode} ({(int)this.StatusCode}): {this.Response}";
}
