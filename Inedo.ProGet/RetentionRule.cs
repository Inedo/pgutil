namespace Inedo.ProGet;

public sealed class RetentionRule
{
    public bool DeletePrereleaseVersions { get; set; }

    public int? KeepVersionsCount { get; set; }

    public int? KeepUsedWithinDays { get; set; }

    public string[]? DeletePackageIds { get; set; }

    public string[]? KeepPackageIds { get; set; }

    public string[]? KeepVersions { get; set; }

    public string[]? DeleteVersions { get; set; }

    public bool DeleteCached { get; set; }

    public long? SizeTriggerKb { get; set; }

    public bool SizeExclusive { get; set; }

    public int? TriggerDownloadCount { get; set; }

    public int? KeepConsumedWithinDays { get; set; }

    public bool KeepIfActivelyConsumed { get; set; }

    public int? KeepPackageUsageRemovedDays { get; set; }
}
