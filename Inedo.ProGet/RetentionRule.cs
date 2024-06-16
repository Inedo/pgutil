/*******************************************************************************
* ABOUT THIS FILE                                                              *
********************************************************************************
*                                                                              *
* This file contains C# code to read and write JSON                            *
* objects for ProGet HTTP Endpoints. It also serves as the specifications for  *
* the expected format of these JSON objects; this is why it doesn't follow     *
* normal C# commenting contentions/standards.                                  *
*                                                                              *
* If you're not familiar with JSON Serialization in C#, a few notes:           *
*                                                                              *
* - The C# property names are PascalCase, but they are converted to camelCase  *
*   for JSON; e.g. "MyProperty" will become "myProperty" unless otherwise      *
*   overridden by a JsonPropertyName attribute
*                                                                              *
* - Some C# properties will reference enums; these are converted to camelCase  *
*   string values; e.g. "MyValue" will become "myValue"                        *
*                                                                              *
* - Date types are forgiving, but you should specify ISO8601 and C# will       *
*   output something like "2019-08-01T00:00:00-07:00"                          *
*                                                                              *
* - A type with a ? (e.g. int?) means that the JSON property may be missing    *
*   or null. This usually means that it's an optional property, but it also    *
*   could mean that it's required only in some contexts                        *
*                                                                              *
* - The "required" keyword means that the JSON property should always be       *
*   present (when listing) or must be specified when creating/editing          *
*                                                                              *
*******************************************************************************/

namespace Inedo.ProGet;

// JSON Object used by the ProgetFeed object
// Properties marked with (W) support wildcards (but not negations)
public sealed class RetentionRule
{
    // When "true", the retention rule only applies to pre-release packages 
    // * E.g., they have a pre-release part in their semantic version, or are SNAPSHOT versions
    public bool DeletePrereleaseVersions { get; set; }

    // When set to "n", the retention rule always keeps the latest "n" versions of a matching package
    // * This value is ignored for Docker feeds
    public int? KeepVersionsCount { get; set; }

    // When set to "n", the retention rule always keeps package versions if they have been downloaded within the past "n" days
    public int? KeepUsedWithinDays { get; set; }

    // Array of package names/identifiers that are deleted if they match all other filters (W)
    // * Use "keepPackageIds" for exclusions
    public string[]? DeletePackageIds { get; set; }

    // Array of package names/identifiers that are kept regardless of other filters (W)
    public string[]? KeepPackageIds { get; set; }

    // Array of package versions that are kept regardless of other filters (W)
    public string[]? KeepVersions { get; set; }

    // Array of package versions that are deleted if they match all other filters (W)
    // * Use `keepVersions` for exclusions
    public string[]? DeleteVersions { get; set; }

    // When "true", the retention rule only applies to cached connector packages
    public bool DeleteCached { get; set; }

    // When set to "n", indicates the minimum number of kilobytes of storage that must be used in order to run retention
    // * Used storage is calculated per-package or per-feed based on the value of "sizeExclusive"
    public long? SizeTriggerKb { get; set; }

    // When "true" and "sizeTriggerKb" is set to non-null "n", retention is run only on packages whose disk size is greater than `n` kilobytes
    // * When "false" and "sizeTriggerKb" is set to non-null "n", retention is run when the entire feed size is greater than `n` kilobytes
    // * This value is ignored when "sizeTriggerKb" is "null"
    public bool SizeExclusive { get; set; }
    
    // When set to "n", the retention rule always keeps versions that have been downloaded more than "n" times
    public int? TriggerDownloadCount { get; set; }

    public int? KeepConsumedWithinDays { get; set; }

    public bool KeepIfActivelyConsumed { get; set; }

    public int? KeepPackageUsageRemovedDays { get; set; }
}