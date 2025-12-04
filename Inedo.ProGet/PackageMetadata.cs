/*******************************************************************************
* ABOUT THIS FILE                                                              *
********************************************************************************
*                                                                              *
* This file contains C# code to serialize (write) and deserialize (read) JSON  *
* objects for ProGet HTTP Endpoints. It also serves as the specifications for  *
* the expected format of these JSON objects; this is why it doesn't follow     *
* normal C# commenting contentions/standards.                                  *
*                                                                              *
* If you're not familiar with JSON Serialization in C#, a few notes:           *
*                                                                              *
* - The C# property names are PascalCase, but they are converted to camelCase  *
*   for JSON; e.g. "MyProperty" will become "myProperty"                       *
*                                                                              *
* - Some C# properties will reference enums; these are converted to camelCase  *
*   string values; e.g. "MyValue" will become "myValue"                        *
*                                                                              *
* - Date types are forgiving, but you should specify ISO8601 and C# will       *
*   outputs something like "2019-08-01T00:00:00-07:00"                         *
*                                                                              *
* - A type with a ? (e.g. int?) means that the JSON property may be missing    *
*   or null. This usually means that it's an optional property, but it also    *
*   could mean that it's required only in some contexts                        *
*                                                                              *
* - The "required" keyword means that the JSON property should always be       *
*   present (when listing) or must be specified when creating/editing          *
*                                                                              *
*******************************************************************************/

#nullable enable

namespace Inedo.ProGet;

// JSON Object used by the Show Metadata HTTP endpoint
public sealed class PackageMetadata
{
    // Name of the package (e.g. "Newtonsoft.Json")
    public required string Name { get; init; }

    // Version of the package (e.g. "13.0.3")
    public required string Version { get; init; }

    // Date the package was published (e.g. "2023-01-15T12:34:56Z")
    public DateTime? Published { get; init; }

    // User who published the package (e.g. "John Doe")
    public string? PublishedBy { get; init; }

    // Indicates the state/location of the package (e.g. "Local", "Remote")
    public required string State { get; init; }

    // Size of the package in bytes (e.g. 204800)
    public long? Size { get; init; }

    // Total number of downloads for the package (e.g. 200)
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
