using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Inedo.DependencyScan;

namespace Inedo.ProGet;

public sealed class ProGetClient
{
    private readonly HttpClient http;

    public ProGetClient(string url, ProGetAuthentication? authentication = null, HttpClient? httpClient = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        if (!url.EndsWith('/'))
            url += "/";

        this.http = httpClient ?? new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });
        this.http.BaseAddress = new Uri(url);
        authentication?.SetHeaders(this.http);
    }

    public Task<ProGetInstanceHealth> GetInstanceHealthAsync(CancellationToken cancellationToken = default)
    {
        return this.http.GetFromJsonAsync("health", ProGetApiJsonContext.Default.ProGetInstanceHealth, cancellationToken)!;
    }

    public IAsyncEnumerable<ProGetFeed> ListFeedsAsync(CancellationToken cancellationToken = default)
    {
        return this.http.GetFromJsonAsAsyncEnumerable("api/management/feeds/list", ProGetApiJsonContext.Default.ProGetFeed, cancellationToken)!;
    }
    public async Task<ProGetFeed> CreateFeedAsync(string feedName, string feedType, CancellationToken cancellationToken = default)
    {
        var input = new ProGetFeed { Name = feedName, FeedType = feedType };
        using var response = await this.http.PostAsJsonAsync("api/management/feeds/create", input, ProGetApiJsonContext.Default.ProGetFeed, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(stream, ProGetApiJsonContext.Default.ProGetFeed, cancellationToken).ConfigureAwait(false))!;
    }
    public async Task DeleteFeedAsync(string feedName, CancellationToken cancellationToken = default)
    {
        using var response = await this.http.DeleteAsync($"api/management/feeds/delete/{Uri.EscapeDataString(feedName)}", cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken);
    }
    public async Task SetPackageStatusAsync(PackageIdentifier package, PackageStatus status, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(status);

        var url = GetPackageUrl($"api/packages/{Uri.EscapeDataString(package.Feed)}/status", package);
        using var response = await this.http.PostAsJsonAsync(url, status, ProGetApiJsonContext.Default.PackageStatus, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task PublishSbomAsync(IEnumerable<ScannedProject> projects, PackageConsumer consumer, string consumerType, string packageType, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(projects);
        ArgumentNullException.ThrowIfNull(consumer);
        ArgumentException.ThrowIfNullOrEmpty(consumerType);
        ArgumentException.ThrowIfNullOrEmpty(packageType);

        using var temp = new MemoryStream();
        BomWriter.WriteSbom(temp, projects, consumer, consumerType, packageType);
        if (!temp.TryGetBuffer(out var buffer))
            throw new InvalidOperationException();

        using var content = new ReadOnlyMemoryContent(buffer.AsMemory());
        content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");

        using var response = await this.http.PostAsync("api/sca/import", content, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken);
    }

    public async Task AnalyzeBuildAsync(string projectName, string buildNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(buildNumber);

        using var response = await this.http.PostAsync($"api/sca/analyze-build?project={Uri.EscapeDataString(projectName)}&version={Uri.EscapeDataString(buildNumber)}", null, cancellationToken);
        await CheckResponseAsync(response, cancellationToken);
    }
    public async Task PromoteBuildAsync(string projectName, string buildNumber, string stageName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(buildNumber);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        using var response = await this.http.PostAsync($"api/sca/promote-build?project={Uri.EscapeDataString(projectName)}&version={Uri.EscapeDataString(buildNumber)}&stage={Uri.EscapeDataString(stageName)}", null, cancellationToken);
        await CheckResponseAsync(response, cancellationToken);
    }

    public async Task<PackageDownloadStream> DownloadPackageAsync(PackageIdentifier package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        var url = GetPackageUrl($"api/packages/{Uri.EscapeDataString(package.Feed)}/download", package);
        var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        try
        {
            await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
            return new PackageDownloadStream(response, await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }
    public async Task UploadPackageAsync(Stream source, string feed, Action<long>? reportProgress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(feed);

        var url = $"api/packages/{Uri.EscapeDataString(feed)}/upload";
        using var content = getContent();
        using var response = await this.http.PutAsync(url, content, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        StreamContent getContent()
        {
            if (reportProgress is null)
                return new StreamContent(source);
            else
                return new StreamContent(new PackageUploadStream(source, reportProgress));
        }
    }

    public async Task DeletePackageAsync(PackageIdentifier package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        var url = GetPackageUrl($"api/packages/{Uri.EscapeDataString(package.Feed)}/delete", package);
        using var response = await this.http.PostAsync(url, null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static async Task CheckResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new ProGetClientException(response.StatusCode, message);
    }
    private static string GetPackageUrl(string baseUrl, PackageIdentifier package)
    {
        var url = $"{baseUrl}?name={Uri.EscapeDataString(package.Name)}&version={Uri.EscapeDataString(package.Version)}";

        if (!string.IsNullOrEmpty(package.Group))
            url += $"&group={Uri.EscapeDataString(package.Group)}";
        if (!string.IsNullOrEmpty(package.Qualifier))
            url += $"&qualifier={Uri.EscapeDataString(package.Qualifier)}";

        return url;
    }
}
