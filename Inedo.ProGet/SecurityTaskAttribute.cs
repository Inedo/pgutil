namespace Inedo.ProGet;

public sealed class SecurityTaskAttribute
{
    public required string Name { get; init; }
    public bool FeedScoped { get; init; }
}
