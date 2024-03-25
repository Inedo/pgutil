namespace Inedo.ProGet;

public sealed class ProGetApiKeyAuthentication : ProGetAuthentication
{
    public ProGetApiKeyAuthentication(string apiKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        this.ApiKey = apiKey;
    }

    public string ApiKey { get; }

    public override string ToString() => "API Key";

    protected internal override void SetHeaders(HttpClient httpClient) => httpClient.DefaultRequestHeaders.Add("X-ApiKey", this.ApiKey);
}
