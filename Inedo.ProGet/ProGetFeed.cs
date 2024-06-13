namespace Inedo.ProGet;

public sealed class ProGetFeed
{
    public string? Name { get; set; }
    public string[]? AlternateNames { get; set; }
    public string? FeedType { get; set; }
    public string? Description { get; set; }
    public bool? Active { get; set; }
    public bool? CacheConnectors { get; set; }
    public string? DropPath { get; set; }
    public string? PackagesPath { get; set; }
    public string? PackageStore { get; set; }
    public bool? AllowUnknownLicenses { get; set; }
    public string[]? AllowedLicenses { get; set; }
    public string[]? BlockedLicenses { get; set; }
    public bool? SymbolServerEnabled { get; set; }
    public bool? StripSymbols { get; set; }
    public bool? StripSignature { get; set; }
    public bool? UseApiV3 { get; set; }
    public string? EndpointUrl { get; set; }
    public string[]? Connectors { get; set; }
    public string[]? VulnerabilitySources { get; set; }
    public RetentionRule[]? RetentionRules { get; set; }
    public Dictionary<string, string>? PackageFilters { get; set; }
    public Dictionary<string, string>? PackageAccessRules { get; set; }
    public Dictionary<string, string>? VariableDictionary { get; set; }
    public bool? CanPublish { get; set; }
    public bool? PackageStatisticsEnabled { get; set; }
    public bool? RestrictPackageStatistics { get; set; }
    public bool? DeploymentRecordsEnabled { get; set; }
    public bool? UsageRecordsEnabled { get; set; }
    public bool? VulnerabilitiesEnabled { get; set; }
    public bool? LicensesEnabled { get; set; }
    public bool? UseWithProjects { get; set; }
    public bool? RetentionRulesEnabled { get; set; }
}
