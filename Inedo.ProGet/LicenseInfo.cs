namespace Inedo.ProGet;

public sealed class LicenseInfo
{
    public int? Id { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public IReadOnlyList<string>? Spdx { get; init; }
    public IReadOnlyList<string>? Urls { get; init; }
}
