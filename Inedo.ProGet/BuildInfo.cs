namespace Inedo.ProGet;

public sealed class BuildInfo
{
    public required string Version { get; init; }
    public bool Active { get; init; }
    public string? Url { get; init; }
    public required string ViewBuildUrl { get; init; }
    public BuildComment[]? Comments { get; init; }
    public BuildPackage[]? Packages { get; init; }
}
