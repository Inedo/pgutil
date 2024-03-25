namespace Inedo.ProGet;

public sealed record class ProGetInstanceHealth(string ApplicationName, string DatabaseStatus, string? DatabaseStatusDetails, string LicenseStatus, string? LicenseStatusDetail, string VersionNumber, string ReleaseNumber, string ServiceStatus, string? ServiceStatusDetail);
