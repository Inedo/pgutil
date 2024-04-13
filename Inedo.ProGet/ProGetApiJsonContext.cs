using System.Text.Json.Serialization;

namespace Inedo.ProGet;

[JsonSerializable(typeof(ProGetFeed))]
[JsonSerializable(typeof(PackageStatus))]
[JsonSerializable(typeof(ProGetInstanceHealth))]
[JsonSerializable(typeof(VulnerabilityInfo))]
[JsonSerializable(typeof(IReadOnlyList<PackageVersionIdentifier>))]
[JsonSerializable(typeof(RepackageInput))]
[JsonSerializable(typeof(PromotePackageInput))]
[JsonSerializable(typeof(BuildAnalysisResults))]
[JsonSerializable(typeof(AuditPackageResults))]
[JsonSerializable(typeof(LicenseInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ProGetApiJsonContext : JsonSerializerContext
{
}
