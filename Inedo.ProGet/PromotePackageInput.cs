namespace Inedo.ProGet;

public sealed class PromotePackageInput
{
    public required string FromFeed { get; init; }
    public string? Group { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string ToFeed { get; init; }
    public string? Comments { get; init; }
}
