using System.Text.Json.Serialization;

namespace Inedo.ProGet;

public sealed class BuildPackage
{
    [JsonPropertyName("purl")]
    public required string PUrl { get; init; }
    public required string[] Licenses { get; init; }
    public required BuildPackageComplianceInfo Compliance { get; init; }
    public required BuildPackageVulnerability Vulnerabilities { get; init; }
}
