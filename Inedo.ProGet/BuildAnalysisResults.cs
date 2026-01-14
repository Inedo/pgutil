/*******************************************************************************
* ABOUT THIS FILE                                                              *
********************************************************************************
*                                                                              *
* This file contains C# code to serialize (write) and deserialize (read) JSON  *
* objects for ProGet HTTP Endpoints. It also serves as the specifications for  *
* the expected format of these JSON objects; this is why it doesn't follow     *
* normal C# commenting contentions/standards.                                  *
*                                                                              *
* If you're not familiar with JSON Serialization in C#, a few notes:           *
*                                                                              *
* - The C# property names are PascalCase, but they are converted to camelCase  *
*   for JSON; e.g. "MyProperty" will become "myProperty"                       *
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

// JSON Object used by the Audit Build HTTP endpoint
public sealed class BuildAnalysisResults
{
    // Date and time the analysis was last performed
    // * Date is output in ISO8601 format (e.g. "2019-08-01T00:00:00-07:00")
    public DateTime? LastAnalyzedDate { get; init; }

    // Status code of the analysis
    // * Values are either "C" (Compliant), "W" (Warn), or "N" (Noncompliant)
    public string? StatusCode { get; init; }

    // Number of issues found during the analysis (e.g. 5)
    public int? IssueCount { get; init; }

    // Number of unresolved issues found during the analysis (e.g. 2)
    public int? UnresolvedIssueCount { get; init; }

    // Status text of the analysis
    // * Will be either "Compliant", "Warn", or "Noncompliant"
    public required string StatusText { get; init; }

    // Total number of packages analyzed in the build (e.g. 42)
    public int? TotalPackages { get; init; }
}
