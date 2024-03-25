using System.Net.Http.Headers;
using System.Text;

namespace Inedo.ProGet;

public sealed class ProGetBasicAuthentication : ProGetAuthentication
{
    public ProGetBasicAuthentication(string user, string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(user);
        ArgumentException.ThrowIfNullOrEmpty(password);
        this.User = user;
        this.Password = password;
    }

    public string User { get; }
    public string Password { get; }

    public override string ToString() => $"basic {this.User}:<password>";

    protected internal override void SetHeaders(HttpClient httpClient)
    {
        var utf8 = new UTF8Encoding(false);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(utf8.GetBytes($"{this.User}:{this.Password}")));
    }
}