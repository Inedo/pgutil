#nullable enable

namespace Inedo.ProGet;

public sealed class AuditPackageResults
{
    public string? ResultCode { get; init; }
    public DateTime? AnalysisDate { get; init; }
    public string? Detail { get; init; }
    public required string StatusText { get; init; }
}
