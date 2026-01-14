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

    // Date the package was published
    // * Written in ISO8601 format (e.g. "2023-01-15T12:34:56Z")
    public DateTime? Published { get; init; }

    // User who published the package (e.g. "John Doe")
    public string? PublishedBy { get; init; }

    // Indicates the state/location of the package (e.g. "Local", "Remote")
    public required string State { get; init; }

    // Size of the package in bytes (e.g. 204800)
    public long? Size { get; init; }

    // Total number of downloads for the package (e.g. 200)
    public long? Downloads { get; init; }

    // Artifact details, see "PackageMetadataArtifact" below for more information
    // * May be omitted if the package type does not have any associated artifacts
    public PackageMetadataArtifact[]? Artifacts { get; init; }

    // Vulnerability details, see "PackageMetadataVulnerability" below for more information
    public PackageMetadataVulnerability[]? Vulnerabilities { get; init; }

    // Compliance report details, see "PackageMetadataCompliance" below for more information
    public PackageMetadataCompliance? ComplianceReport { get; init; }

    // License details, see "PackageMetadataLicense" below for more information
    public PackageMetadataLicense[]? Licenses { get; init; }

    // Status details, see "PackageMetadataStatus" below for more information
    public PackageMetadataStatus? Status { get; init; }
}

public sealed class PackageMetadataArtifact
{
    // Unique identifier used to distinguish individual artifacts of a package (e.g. "file=requests-2.31.0.tar.gz")
    public required string Qualifier { get; init; }

    // Name of the artifact (e.g. "requests-2.31.0.tar.gz")
    public string? Name { get; init; }

    // Size of the artifact in bytes (e.g. 123456)
    public long? Size { get; init; }

    // Total number of downloads for the artifact (e.g. 150)
    public long? Downloads { get; init; }

    // Date the artifact was published
    // * Written in ISO8601 format (e.g. "2023-01-15T12:34:56Z")
    public DateTime? Published { get; init; }

    // User who published the artifact (e.g. "jsmith")
    public string? PublishedBy { get; init; }

    // State/location of the artifact (e.g. "Local", "Remote")
    public required string State { get; init; }

    // Status details, see "PackageMetadataStatus" below for more information
    public PackageMetadataStatus? Status { get; init; }
}

public sealed class PackageMetadataVulnerability
{
    // Unique identifier of the vulnerability (e.g. "CVE-2023-12345")
    public required string Id { get; init; }

    // Title/summary of the vulnerability (e.g. "Buffer Overflow in XYZ Library")
    public required string Summary { get; init; }

    // CVSS score of the vulnerability (e.g. 7.5)
    public decimal? Score { get; init; }

    // Vulnerability assessments, see "PackageMetadataVulnerabilityAssessment" below for more information
    public PackageMetadataVulnerabilityAssessment[]? Assessments { get; init; }
}

public sealed class PackageMetadataVulnerabilityAssessment
{
    // Assessment Type
    // * Defaults are "Caution", "Blocked", or "Ignore"
    // * Custom types may also be used
    public required string Type { get; init; }

    // Severity of the assessment (E.g. "W", "E")
    public required string Severity { get; init; }
    
    // Indicates whether the vulnerability is blocked
    // * Values are either "true" or "false"
    public bool Blocked { get; init; }

    // Date the assessment was made
    // * Written in ISO8601 format (e.g. "2023-01-15T12:34:56Z")
    public required DateTime Date { get; init; }

    // User who made the assessment (e.g. "admin")
    public required string By { get; init; }
}

public sealed class PackageMetadataCompliance
{
    // Date the analysis was performed
    // * Written in ISO8601 format (e.g. "2023-01-15T12:34:56Z")
    public required DateTime AnalysisDate { get; init; }

    // Result of the analysis
    // * Values are either "Compliant", "Warn", or "Noncompliant"
    public required string AnalysisResult { get; init; }

    // Issues found during the analysis (e.g. "Vulnerability (PGV-2245804)", "Unacceptable License (GPL-3.0)")
    public string[]? Issues { get; init; }
}

public sealed class PackageMetadataLicense
{
    // License code/identifier (e.g. "MIT", "GPL-3.0")
    public required string Code { get; init; }

    // Title/name of the license (e.g. "MIT License", "GNU General Public License v3.0")
    public required string Title { get; init; }
}

public sealed class PackageMetadataStatus
{
    // Indicates whether the package is deprecated
    // * Values are either "true" or "false"
    public bool Deprecated { get; init; }

    // Indicates whether the package is listed (visible to searches)
    // * Values are either "true" or "false"
    public bool Listed { get; init; }

    // Indicates whether downloads are allowed for the package
    // * Values are either "true" or "false"
    public bool? AllowDownload { get; init; }
}
