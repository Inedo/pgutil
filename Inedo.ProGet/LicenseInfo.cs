namespace Inedo.ProGet;

public sealed class LicenseInfo
{
    public int? Id { get; init; }
    public required string Code { get; init; }
    public string? Title { get; init; }
    public IReadOnlyList<string>? Spdx { get; init; }
    public IReadOnlyList<string>? Urls { get; init; }
    public IReadOnlyList<string>? Hashes { get; init; }
    public IReadOnlyList<string>? PackageNames { get; init; }
    public IReadOnlyList<string>? Purls { get; init; }
}
