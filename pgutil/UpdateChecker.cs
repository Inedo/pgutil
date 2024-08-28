using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PgUtil;

internal static class UpdateChecker
{
    private static string VersionFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pgutil", "pgutil.latest");

    public static Version? GetLatestVersion()
    {
        Version? version = null;
        try
        {
            var versionFilePath = VersionFilePath;
            DateTime? timestamp = null;

            if (File.Exists(versionFilePath))
            {
                timestamp = File.GetLastWriteTimeUtc(versionFilePath);
                using var reader = File.OpenText(versionFilePath);
                Span<char> buffer = stackalloc char[50];
                int len = reader.ReadBlock(buffer);
                _ = Version.TryParse(buffer[0..len], out version);
            }

            if (!timestamp.HasValue || DateTime.UtcNow.Subtract(timestamp.GetValueOrDefault()) >= new TimeSpan(1, 0, 0, 0))
                FetchLatestVersion();
        }
        catch
        {
        }

        return version;
    }

    private static async void FetchLatestVersion()
    {
        try
        {
            var version = await GetLatestVersionAsync();
            if (version is not null)
            {
                var path = VersionFilePath;
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, version.ToString());
            }
        }
        catch
        {
        }
    }
    private static async Task<Version?> GetLatestVersionAsync()
    {
        try
        {
            var client = new HttpClient();
            using var message = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/Inedo/pgutil/releases/latest");
            message.Headers.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("pgutil", typeof(Program).Assembly.GetName().Version!.ToString(3))));
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            message.Headers.Add("X-GitHub-Api-Version", "2022-11-28");

            using var response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);
                if (doc.RootElement.TryGetProperty("name"u8, out var nameProperty) && Version.TryParse(nameProperty.GetString(), out var version))
                    return version;
            }
        }
        catch
        {
        }

        return null;
    }
}
