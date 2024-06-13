namespace Inedo.ProGet;

public sealed class RetentionRule
{
    // When `true`, the retention rule only applies to pre-release packages 
    // * E.g., they have a pre-release part in their semantic version, or are SNAPSHOT versions
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


| `deletePrereleaseVersions`| bool |  |
| `deleteCached` | bool | when `true`, the retention rule only applies to cached connector packages |
| `keepVersionsCount` | int | when set to `n`, the retention rule always keeps the latest `n` versions of a matching package; this value is ignored for Docker feeds |
| `keepUsedWithinDays` | int | when set to `n`, the retention rule always keeps package versions if they have been downloaded within the past `n` days |
| `triggerDownloadCount` | int | when set to `n`, the retention rule always keeps versions that have been downloaded more than `n` times |
| `keepPackageIds` | string[] | array of package names/identifiers that are kept regardless of other filters; supports [wildcards](#wildcards) but not negations, as any matching package is always kept |
| `deletePackageIds` | string[] | array of package names/identifiers that are deleted if they match all other filters; supports [wildcards](#wildcards) but not negations (use `keepPackageIds` for exclusions)  |
| `keepVersions` | string[] | array of package versions that are kept regardless of other filters; supports [wildcards](#wildcards) but not negations, as any matching package version is always kept |
| `deleteVersions` | string[] | array of package versions that are deleted if they match all other filters; supports [wildcards](#wildcards) but not negations (use `keepVersions` for exclusions) |
| `sizeTriggerKb` | int | when set to `n`, indicates the minimum number of kilobytes of storage that must be used in order to run retention; used storage is calculated per-package or per-feed based on the value of `sizeExclusive` |
| `sizeExclusive` | bool | when `true` and `sizeTriggerKb` is set to non-null `n`, retention is run only on packages whose disk size is greater than `n` kilobytes; when `false` and `sizeTriggerKb` is set to non-null `n`, retention is run when the entire feed size is greater than `n` kilobytes; this value is ignored when `sizeTriggerKb` is `null`  |