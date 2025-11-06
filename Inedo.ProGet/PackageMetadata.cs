#nullable enable

namespace Inedo.ProGet;

public sealed class PackageMetadata
{
    public required string Name { get; init; }
    public required string Version { get; init; }
    public DateTime? Published { get; init; }
    public string? PublishedBy { get; init; }
    public required string State { get; init; }
    public long? Size { get; init; }
    public long? Downloads { get; init; }
    public PackageMetadataArtifact[]? Artifacts { get; init; }
    public PackageMetadataVulnerability[]? Vulnerabilities { get; init; }
    public PackageMetadataCompliance? ComplianceReport { get; init; }
    public PackageMetadataLicense[]? Licenses { get; init; }
    public PackageMetadataStatus? Status { get; init; }
}

public sealed class PackageMetadataArtifact
{
    public required string Qualifier { get; init; }
    public string? Name { get; init; }
    public long? Size { get; init; }
    public long? Downloads { get; init; }
    public DateTime? Published { get; init; }
    public string? PublishedBy { get; init; }
    public required string State { get; init; }
    public PackageMetadataStatus? Status { get; init; }
}

public sealed class PackageMetadataVulnerability
{
    public required string Id { get; init; }
    public required string Summary { get; init; }
    public decimal? Score { get; init; }
    public PackageMetadataVulnerabilityAssessment[]? Assessments { get; init; }
}

public sealed class PackageMetadataVulnerabilityAssessment
{
    public required string Type { get; init; }
    public required string Severity { get; init; }
    public bool Blocked { get; init; }
    public required DateTime Date { get; init; }
    public required string By { get; init; }
}

public sealed class PackageMetadataCompliance
{
    public required DateTime AnalysisDate { get; init; }
    public required string AnalysisResult { get; init; }
    public string[]? Issues { get; init; }
}

public sealed class PackageMetadataLicense
{
    public required string Code { get; init; }
    public required string Title { get; init; }
}

public sealed class PackageMetadataStatus
{
    public bool Deprecated { get; init; }
    public bool Listed { get; init; }
    public bool? AllowDownload { get; init; }
}
