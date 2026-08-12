namespace Inedo.ProGet;

public sealed class AddTagFullResult
{
    public required AddTagResult Status { get; init; }
    public string? Digest { get; init; }
}
