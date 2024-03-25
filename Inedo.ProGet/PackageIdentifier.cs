namespace Inedo.ProGet;

public sealed record class PackageIdentifier
{
    public required string Feed { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public string? Group { get; init; }
    public string? Qualifier { get; init; }
}
