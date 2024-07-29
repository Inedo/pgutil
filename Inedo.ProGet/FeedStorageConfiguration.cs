namespace Inedo.ProGet;

public sealed class FeedStorageConfiguration
{
    public required string Id { get; init; }
    public required IReadOnlyDictionary<string, object?> Properties { get; init; }
}
