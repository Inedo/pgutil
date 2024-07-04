using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Inedo.DependencyScan;
using Inedo.ProGet.AssetDirectories;

namespace Inedo.ProGet;

public sealed class ProGetClient
{
    private readonly HttpClient http;
    public ProGetAuthenticationType AuthenticationType { get; }
    public string? UserName { get; }
    public string Url { get; }
    public ProGetClient(string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        if (!url.EndsWith('/'))
            url += "/";
        this.Url = url;

        this.http = new(new HttpClientHandler { UseDefaultCredentials = true })
        {
            BaseAddress = new Uri(url)
        };
    }
    public ProGetClient(string url, string username, string password) : this(url, $"{username}:{password}")
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(password); 
        this.AuthenticationType = ProGetAuthenticationType.UsernamePassword;
        this.UserName = username;
    }
    public ProGetClient(string url, string apiKey) : this(url)
    {
        ArgumentException.ThrowIfNullOrEmpty(apiKey);
        this.http.DefaultRequestHeaders.Add("X-ApiKey", apiKey);
        this.AuthenticationType = ProGetAuthenticationType.ApiKey;
    }

    public async Task<ProGetHealthInfo> GetInstanceHealthAsync(CancellationToken cancellationToken = default)
    {
        using var response = await this.http.GetAsync("health", cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        
        try
        {
            return (await JsonSerializer.DeserializeAsync(stream, ProGetApiJsonContext.Default.ProGetHealthInfo, cancellationToken).ConfigureAwait(false))!;
        }
        catch (JsonException jex)
        {
            throw new ProGetApiException(response.StatusCode, $"Unexpected server response ({jex.Message})");
        }
        catch (Exception ex)
        {
            throw new ProGetApiException(response.StatusCode, ex.Message);
        }
    }

    public AssetDirectoryClient GetAssetDirectoryClient(string assetDirectoryName)
    {
        ArgumentException.ThrowIfNullOrEmpty(assetDirectoryName);
        return new AssetDirectoryClient(this.http, assetDirectoryName);
    }

    public IAsyncEnumerable<ProGetFeed> ListFeedsAsync(CancellationToken cancellationToken = default)
    {
        return this.http.GetFromJsonAsAsyncEnumerable("api/management/feeds/list", ProGetApiJsonContext.Default.ProGetFeed, cancellationToken)!;
    }
    public async Task<ProGetFeed> GetFeedAsync(string feedName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(feedName);

        using var response = await this.http.GetAsync($"api/management/feeds/get/{feedName}", cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(stream, ProGetApiJsonContext.Default.ProGetFeed, cancellationToken).ConfigureAwait(false))!;
    }
    public async Task<ProGetFeed> UpdateFeedAsync(string feedName, ProGetFeed feed, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(feedName);
        ArgumentNullException.ThrowIfNull(feed);

        using var response = await this.http.PostAsJsonAsync($"api/management/feeds/update/{feedName}", feed, ProGetApiJsonContext.Default.ProGetFeed, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(stream, ProGetApiJsonContext.Default.ProGetFeed, cancellationToken).ConfigureAwait(false))!;
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

    public async Task CreateConnectorAsync(ProGetConnector connectorInfo, CancellationToken cancellationToken = default)
    {
        using var response = await this.http.PostAsJsonAsync("api/management/connectors/create", connectorInfo, ProGetApiJsonContext.Default.ProGetConnector, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async IAsyncEnumerable<ProGetConnector> ListConnectorsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var response = await this.http.GetAsync("api/management/connectors/list", HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var connector in JsonSerializer.DeserializeAsyncEnumerable(stream, ProGetApiJsonContext.Default.ProGetConnector, cancellationToken).ConfigureAwait(false))
            yield return connector!;
    }
    public async Task<ProGetConnector> GetConnectorAsync(string connectorName, CancellationToken cancellationToken = default)
    {
        using var response = await this.http.GetAsync($"api/management/connectors/get/{connectorName}", cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(stream, ProGetApiJsonContext.Default.ProGetConnector, cancellationToken).ConfigureAwait(false))!;
    }
    public async Task<ProGetConnector> UpdateConnectorAsync(string connectorName, ProGetConnector connector, CancellationToken cancellationToken = default)
    {
        using var response = await this.http.PostAsJsonAsync($"api/management/connectors/update/{connectorName}", connector, ProGetApiJsonContext.Default.ProGetConnector, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(stream, ProGetApiJsonContext.Default.ProGetConnector, cancellationToken).ConfigureAwait(false))!;
    }
    public async Task DeleteConnectorAsync(string connectorName, CancellationToken cancellationToken = default)
    {
        using var response = await this.http.PostAsync($"api/management/connectors/delete/{Uri.EscapeDataString(connectorName)}", null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
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
    public async Task<BuildAnalysisResults> PromoteBuildAsync(string projectName, string buildNumber, string stageName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(buildNumber);
        ArgumentException.ThrowIfNullOrEmpty(stageName);

        using var response = await this.http.PostAsync($"api/sca/promote-build?project={Uri.EscapeDataString(projectName)}&version={Uri.EscapeDataString(buildNumber)}&stage={Uri.EscapeDataString(stageName)}", null, cancellationToken);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(responseStream, ProGetApiJsonContext.Default.BuildAnalysisResults, cancellationToken).ConfigureAwait(false))!;
    }
    public async Task<BuildAnalysisResults> AuditBuildAsync(string projectName, string buildNumber, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(projectName);
        ArgumentException.ThrowIfNullOrEmpty(buildNumber);

        using var response = await this.http.PostAsync($"api/sca/audit-build?project={Uri.EscapeDataString(projectName)}&version={Uri.EscapeDataString(buildNumber)}", null, cancellationToken);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(responseStream, ProGetApiJsonContext.Default.BuildAnalysisResults, cancellationToken).ConfigureAwait(false))!;
    }

    public async Task<BasicFeedInfo> GetBasicFeedInfoAsync(string feed, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(feed);

        var url = $"api/packages/{Uri.EscapeDataString(feed)}";
        using var response = await this.http.GetAsync(url, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize(stream, ProGetApiJsonContext.Default.BasicFeedInfo)!;
    }
    public async IAsyncEnumerable<PackageVersionInfo> ListLatestPackagesAsync(string feed, string? name = null, string? group = null, bool stableOnly = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(feed);

        var url = $"api/packages/{Uri.EscapeDataString(feed)}/latest";

        var filter = new List<string>(2);
        if (!string.IsNullOrEmpty(group))
            filter.Add($"group={Uri.EscapeDataString(group)}");
        if (!string.IsNullOrEmpty(name))
            filter.Add($"name={Uri.EscapeDataString(name)}");
        if (stableOnly)
            filter.Add("stableOnly=true");

        if (filter.Count > 0)
            url += $"?{string.Join('&', filter)}";

        var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var p in JsonSerializer.DeserializeAsyncEnumerable(stream, ProGetApiJsonContext.Default.PackageVersionInfo, cancellationToken).ConfigureAwait(false))
            yield return p!;
    }
    public async IAsyncEnumerable<PackageVersionInfo> ListPackagesAsync(string feed, string? name = null, string? group = null, string? version = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(feed);

        var url = $"api/packages/{Uri.EscapeDataString(feed)}/versions";

        var filter = new List<string>(2);
        if (!string.IsNullOrEmpty(group))
            filter.Add($"group={Uri.EscapeDataString(group)}");
        if (!string.IsNullOrEmpty(name))
            filter.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(version))
            filter.Add($"version={Uri.EscapeDataString(version)}");

        if (filter.Count > 0)
            url += $"?{string.Join('&', filter)}";

        var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var p in JsonSerializer.DeserializeAsyncEnumerable(stream, ProGetApiJsonContext.Default.PackageVersionInfo, cancellationToken).ConfigureAwait(false))
            yield return p!;
    }
    public async Task<ProGetDownloadStream> DownloadPackageAsync(PackageIdentifier package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        var url = GetPackageUrl($"api/packages/{Uri.EscapeDataString(package.Feed)}/download", package);
        var response = await this.http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        try
        {
            await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
            return new ProGetDownloadStream(response, await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }
    public async Task UploadPackageAsync(Stream source, string feed, string? fileName = null, string? distribution = null, Action<long>? reportProgress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(feed);

        var url = $"api/packages/{Uri.EscapeDataString(feed)}/upload";
        if (!string.IsNullOrEmpty(fileName))
            url = $"{url}/{fileName}";
        if (!string.IsNullOrEmpty(distribution))
            url = $"{url}?distribution={Uri.EscapeDataString(distribution)}";

        using var content = getContent();
        using var response = await this.http.PutAsync(url, content, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        StreamContent getContent()
        {
            if (reportProgress is null)
                return new StreamContent(source);
            else
                return new StreamContent(new ProGetUploadStream(source, reportProgress));
        }
    }
    public async Task DeletePackageAsync(PackageIdentifier package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        var url = GetPackageUrl($"api/packages/{Uri.EscapeDataString(package.Feed)}/delete", package);
        using var response = await this.http.PostAsync(url, null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<VulnerabilityInfo> AuditPackagesForVulnerabilitiesAsync(IReadOnlyList<PackageVersionIdentifier> packages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packages);

        using var response = await this.http.PostAsJsonAsync("api/sca/audit-package-vulns", packages, ProGetApiJsonContext.Default.IReadOnlyListPackageVersionIdentifier, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var v in JsonSerializer.DeserializeAsyncEnumerable(stream, ProGetApiJsonContext.Default.VulnerabilityInfo, cancellationToken).ConfigureAwait(false))
            yield return v!;
    }
    public async Task AssessVulnerabilityAsync(string vulnerabilityId, string assessmentType, string? comment = null, string? policy = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(vulnerabilityId);
        ArgumentException.ThrowIfNullOrEmpty(assessmentType);

        var url = $"api/sca/assess?id={Uri.EscapeDataString(vulnerabilityId)}&type={Uri.EscapeDataString(assessmentType)}";
        if (!string.IsNullOrWhiteSpace(comment))
            url = $"{url}&comment={Uri.EscapeDataString(comment)}";
        if (!string.IsNullOrEmpty(policy))
            url = $"{url}&policy={Uri.EscapeDataString(policy)}";

        using var response = await this.http.PostAsync(url, null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task RepackageAsync(RepackageInput repackageInput, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repackageInput);

        using var response = await this.http.PostAsJsonAsync("api/repackaging/repackage", repackageInput, ProGetApiJsonContext.Default.RepackageInput, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async Task PromotePackageAsync(PromotePackageInput promotePackageInput, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(promotePackageInput);

        using var response = await this.http.PostAsJsonAsync("api/promotions/promote", promotePackageInput, ProGetApiJsonContext.Default.PromotePackageInput, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async Task<AuditPackageResults> AuditPackageAsync(PackageIdentifier package, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);

        var url = GetPackageUrl($"api/packages/{Uri.EscapeDataString(package.Feed)}/audit", package);
        using var response = await this.http.PostAsync(url, null, cancellationToken).ConfigureAwait(false);

        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(responseStream, ProGetApiJsonContext.Default.AuditPackageResults, cancellationToken).ConfigureAwait(false))!;
    }

    public IAsyncEnumerable<LicenseInfo> ListLicensesAsync(CancellationToken cancellationToken = default)
    {
        return this.http.GetFromJsonAsAsyncEnumerable("api/licenses/list", ProGetApiJsonContext.Default.LicenseInfo, cancellationToken)!;
    }
    public async Task AddLicenseAsync(LicenseInfo license, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(license);

        using var response = await this.http.PostAsJsonAsync("api/licenses/add", license, ProGetApiJsonContext.Default.LicenseInfo, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async Task DeleteLicenseAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);

        using var response = await this.http.PostAsync($"api/licenses/delete?code={Uri.EscapeDataString(code)}", null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async Task AddLicenseFileAsync(string code, Stream licenseFile, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(code);
        ArgumentNullException.ThrowIfNull(licenseFile);

        using var content = new StreamContent(licenseFile);
        using var response = await this.http.PostAsync($"api/licenses/files/add?code={Uri.EscapeDataString(code)}", content, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async Task DeleteLicenseFileAsync(string hash, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(hash);

        using var response = await this.http.PostAsync($"api/licenses/files/delete?hash={Uri.EscapeDataString(hash)}", null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    public IAsyncEnumerable<ApiKeyInfo> ListApiKeysAsync(CancellationToken cancellationToken = default)
    {
        return this.http.GetFromJsonAsAsyncEnumerable("api/api-keys/list", ProGetApiJsonContext.Default.ApiKeyInfo, cancellationToken )!;
    }
    public async Task DeleteApiKeyAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await this.http.PostAsync($"api/api-keys/delete?id={id}", null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async Task<ApiKeyInfo> CreateApiKeyAsync(ApiKeyInfo apiKeyInfo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(apiKeyInfo);

        var versionRequired = apiKeyInfo.Type == ApiKeyType.Feed ? new Version(24, 0, 3) : null;
        var editionRequired = apiKeyInfo.Type != ApiKeyType.Personal ? ProGetEdition.Basic : (ProGetEdition?)null;

        using var response = await this.http.PostAsJsonAsync("api/api-keys/create", apiKeyInfo, ProGetApiJsonContext.Default.ApiKeyInfo, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, versionRequired, editionRequired, cancellationToken).ConfigureAwait(false);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return (await JsonSerializer.DeserializeAsync(stream, ProGetApiJsonContext.Default.ApiKeyInfo, cancellationToken).ConfigureAwait(false))!;
    }

    public async IAsyncEnumerable<SettingsInfo> ListSettingsAsync(bool showAll = false, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        using var response = await this.http.GetAsync($"api/settings/list?showAll={showAll}", cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, new Version(24, 0, 7), null, cancellationToken).ConfigureAwait(false);

        using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        await foreach (var value in JsonSerializer.DeserializeAsyncEnumerable(content, ProGetApiJsonContext.Default.SettingsInfo, cancellationToken))
        {
            if (value is not null)
                yield return value;
        }        
    }
    public async Task SetSettingAsync(string name, string? value, CancellationToken cancellationToken = default)
    {
        using var response = await this.http.PostAsync($"api/settings/set?name={name}&value={value}", null, cancellationToken).ConfigureAwait(false);
        await CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    internal static void CheckResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        using var stream = response.Content.ReadAsStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        throw new ProGetApiException(response.StatusCode, reader.ReadToEnd());
    }
    internal static Task CheckResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        => CheckResponseAsync(response, null, null, cancellationToken);
    private static async Task CheckResponseAsync(HttpResponseMessage response, Version? versionRequired, ProGetEdition? editionRequired, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        if (versionRequired is not null
            && response.Headers.TryGetValues("X-ProGet-Version", out var versionHeaders)
            && Version.TryParse(versionHeaders.FirstOrDefault(), out var version)
            && versionRequired > version)
            throw new ProGetVersionRequiredException(versionRequired, version);

        if (editionRequired is not null
            && response.Headers.TryGetValues("X-ProGet-Edition", out var editionHeaders)
            && Enum.TryParse<ProGetEdition>(editionHeaders.FirstOrDefault(), true, out var edition)
            && edition <= editionRequired)
            throw new ProGetEditionRequiredException(editionRequired.Value, edition);

        var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new ProGetApiException(response.StatusCode, message);
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

    private enum ProGetEdition { Free, Basic, Enterprise }

    private sealed class ProGetVersionRequiredException(Version requiredVersion, Version actualVersion) : ProGetClientException
    {
        private static string FormatVersion(Version v) => $"20{v.Major}.{v.Build}";
        public override string Message => $"ProGet {FormatVersion(requiredVersion)} is required for the specified command and options but the server reported: ProGet {FormatVersion(actualVersion)}";
    }
    private sealed class ProGetEditionRequiredException(ProGetEdition requiredEdition, ProGetEdition actualEdition) : ProGetClientException
    {
        public override string Message => $"ProGet {requiredEdition} Edition is required for the specified command and options but the server reported: {actualEdition} Edition";
    }
}
