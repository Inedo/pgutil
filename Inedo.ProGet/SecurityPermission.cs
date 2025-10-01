namespace Inedo.ProGet;

public sealed class SecurityPermission
{
    public int? Id { get; init; }
    public string? User { get; init; }
    public string? Group { get; init; }
    public string? Task { get; init; }
    public string? Feed { get; init; }
    public string? FeedGroup { get; init; }
    public bool Deny { get; init; }
}
