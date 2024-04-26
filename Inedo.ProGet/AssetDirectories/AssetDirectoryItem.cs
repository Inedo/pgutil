namespace Inedo.ProGet.AssetDirectories;

public sealed class AssetDirectoryItem
{
    public required string Name { get; init; }
    public string? Parent { get; init; }
    public long? Size { get; init; }
    public required string Type { get; init; }
    public string? Content { get; init; }
    public DateTime Created { get; init; }
    public DateTime Modified { get; init; }
    public AssetUserMetadata[]? UserMetadata { get; init; }
    public bool Directory => this.Type == "dir";
}
