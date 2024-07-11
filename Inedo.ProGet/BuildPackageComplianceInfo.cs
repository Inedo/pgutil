namespace Inedo.ProGet;

public sealed class BuildPackageComplianceInfo
{
    public required string Result { get; init; }
    public string? Detail { get; init; }
    public DateTime? Date { get; init; }
}
