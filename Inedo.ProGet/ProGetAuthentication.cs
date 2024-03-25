namespace Inedo.ProGet;

public abstract class ProGetAuthentication
{
    protected ProGetAuthentication()
    {
    }

    protected internal abstract void SetHeaders(HttpClient httpClient);
}
