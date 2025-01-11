namespace Inedo.ProGet;

public sealed class UniversalPackageInfo
{
    public string? Group { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
}
