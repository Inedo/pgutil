namespace Inedo.ProGet;
public sealed class ProGetHealthInfo
{
    public required string ApplicationName { get; init; }
    public required string DatabaseStatus { get; init; }
    public string? DatabaseStatusDetails { get; init; }
    public required string LicenseStatus { get; init; }
    public string? LicenseStatusDetail { get; init; }
    public required string VersionNumber { get; init; }
    public required string ReleaseNumber { get; init; }
    public required string ServiceStatus { get; init; }
    public string? ServiceStatusDetail { get; init; }
    public ReplicationStatusInfo? ReplicationStatus { get; init; }

    public sealed class ReplicationStatusInfo
    {
        public string? ServerStatus { get; init; }
        public string? ServerError { get; init; }
        public string? ClientStatus { get; init; }
        public string? ClientError { get; init; }
    }
}