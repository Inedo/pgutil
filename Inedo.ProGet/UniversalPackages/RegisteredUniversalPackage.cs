using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inedo.ProGet.UniversalPackages;

public sealed class RegisteredUniversalPackage
{
    public string? Group { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Path { get; init; }
    public string? FeedUrl { get; init; }
    public string? InstallationDate { get; init; }
    public string? InstallationReason { get; init; }
    public string? InstalledUsing { get; init; }
    public string? InstalledBy { get; init; }
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
