namespace Inedo.ProGet;

public sealed class BasicFeedInfo
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string FeedType { get; init; }
    public required string PackageType { get; init; }
}
