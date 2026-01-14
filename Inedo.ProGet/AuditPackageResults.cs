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

#nullable enable

namespace Inedo.ProGet;

// JSON Object used by the Audit Package HTTP endpoint
public sealed class AuditPackageResults
{
    // Code of the package analysis
    // * Values are either "C" (Compliant), "W" (Warn), or "N" (Non-Compliant)
    public string? ResultCode { get; init; }

    // Date that the analysis was performed 
    // * Date is output in ISO8601 format (e.g. "2019-08-01T00:00:00-07:00")
    public DateTime? AnalysisDate { get; init; }

    // Details of the analysis (e.g. ""Vulnerability (PGV-2245804)", "Unacceptable License (GPL-3.0)")
    // * Will be "null" if ResultCode is "C" (Compliant)
    public string? Detail { get; init; }

    // The status text of the analysis
    // * Will be either "Compliant", "Warn", or "Non-Compliant"
    public required string StatusText { get; init; }
}
