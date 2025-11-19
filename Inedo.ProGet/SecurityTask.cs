namespace Inedo.ProGet;

public sealed class SecurityTask
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool FeedScoped { get; init; }
    public required string[] Attributes { get; init; }
}
