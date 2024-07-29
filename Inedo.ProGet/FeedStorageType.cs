namespace Inedo.ProGet;

public sealed class FeedStorageType
{
    public required string Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public IReadOnlyDictionary<string, FeedStorageTypePropertyInfo>? Properties { get; init; }
}
