namespace Inedo.ProGet.AssetDirectories;

internal sealed class AssetItemMetadataUpdate
{
    public string? Type { get; init; }
    public string? UserMetadataUpdateMode { get; init; }
    public IReadOnlyDictionary<string, AssetUserMetadata>? UserMetadata { get; init; }
}
