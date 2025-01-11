﻿using System.Text.Json.Serialization;
using Inedo.ProGet.AssetDirectories;

namespace Inedo.ProGet;

[JsonSerializable(typeof(ProGetFeed))]
[JsonSerializable(typeof(ProGetConnector))]
[JsonSerializable(typeof(PackageStatus))]
[JsonSerializable(typeof(ProGetHealthInfo))]
[JsonSerializable(typeof(VulnerabilityInfo))]
[JsonSerializable(typeof(IReadOnlyList<PackageVersionIdentifier>))]
[JsonSerializable(typeof(RepackageInput))]
[JsonSerializable(typeof(PromotePackageInput))]
[JsonSerializable(typeof(BuildAnalysisResults))]
[JsonSerializable(typeof(AuditPackageResults))]
[JsonSerializable(typeof(LicenseInfo))]
[JsonSerializable(typeof(ApiKeyInfo))]
[JsonSerializable(typeof(AssetDirectoryItem))]
[JsonSerializable(typeof(AssetItemMetadataUpdate))]
[JsonSerializable(typeof(PackageVersionInfo))]
[JsonSerializable(typeof(SettingsInfo))]
[JsonSerializable(typeof(BasicFeedInfo))]
[JsonSerializable(typeof(ProjectInfo))]
[JsonSerializable(typeof(BuildIssue))]
[JsonSerializable(typeof(BuildComment))]
[JsonSerializable(typeof(BuildCommentCreateInfo))]
[JsonSerializable(typeof(BuildInfo))]
[JsonSerializable(typeof(CreateOrUpdateBuildOptions))]
[JsonSerializable(typeof(FeedStorageType))]
[JsonSerializable(typeof(FeedStorageConfiguration))]
[JsonSerializable(typeof(UniversalPackageInfo))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, Converters = [typeof(AssetUserMetadataTypeConverter)], UseStringEnumConverter = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class ProGetApiJsonContext : JsonSerializerContext
{
}
