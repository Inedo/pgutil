namespace Inedo.ProGet;

public sealed class BuildAnalysisResults
{
    public DateTime? LastAnalyzedDate { get; init; }
    public string? StatusCode { get; init; }
    public int? IssueCount { get; init; }
    public int? UnresolvedIssueCount { get; init; }
    public required string StatusText { get; init; }
}
