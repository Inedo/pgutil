namespace Inedo.ProGet;

public sealed record class ProGetInstanceHealth(string ApplicationName, string DatabaseStatus, string? DatabaseStatusDetails, string LicenseStatus, string? LicenseStatusDetail, string VersionNumber, string ReleaseNumber, string ServiceStatus, string? ServiceStatusDetail, ProGetInstanceHealth.ReplicationStatusInfo? ReplicationStatus)
{
    public sealed record class ReplicationStatusInfo(string? ServerStatus, string? ServerError, string? ClientStatus, string? ClientError);
    public bool AllOK => OK(this.DatabaseStatus, this.LicenseStatus, this.ServiceStatus, this.ReplicationStatus?.ServerStatus, this.ReplicationStatus?.ClientStatus);

    private static bool OK(params string?[] ss) => ss.All(s => (s ?? "OK") == "OK");
};

