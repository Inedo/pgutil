namespace Inedo.ProGet;

public sealed class CreateOrUpdateBuildOptions
{
    public required string Project { get; init; }
    public required string Version { get; init; }
    public string? Url { get; init; }
    public bool? Active { get; init; }
    public string? Stage { get; init; }
}
