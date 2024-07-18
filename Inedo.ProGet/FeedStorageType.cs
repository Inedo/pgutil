namespace Inedo.ProGet;

public sealed class FeedStorageType
{
    public required string Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public IReadOnlyDictionary<string, FeedStorageTypePropertyInfo>? Properties { get; init; }
}

public sealed class FeedStorageTypePropertyInfo
{
    public bool Required { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Type { get; init; }
    public string? Placeholder { get; init; }
}

public sealed class FeedStorageConfiguration
{
    public required string Id { get; init; }
    public required IReadOnlyDictionary<string, object?> Properties { get; init; }
}
