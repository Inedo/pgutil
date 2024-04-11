namespace Inedo.ProGet;

public sealed class RepackageInput
{
    public required string Feed { get; init; }
    public string? Group { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string NewVersion { get; init; }
    public string? Comments { get; init; }
    public string? ToFeed { get; init; }
}
