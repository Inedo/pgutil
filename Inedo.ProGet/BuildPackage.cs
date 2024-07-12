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
*   outputs something like "2019-08-01T00:00:00-07:00"                         *
*                                                                              *
* - A type with a ? (e.g. int?) means that the JSON property may be missing    *
*   or null. This usually means that it's an optional property, but it also    *
*   could mean that it's required only in some contexts                        *
*                                                                              *
* - The "required" keyword means that the JSON property should always be       *
*   present (when listing) or must be specified when creating/editing          *
*                                                                              *
*******************************************************************************/

using System.Text.Json.Serialization;

namespace Inedo.ProGet;

// JSON Object used by BuildInfo.cs
public sealed class BuildPackage
{
    [JsonPropertyName("purl")]

    // A package url of the package
    // See https://github.com/package-url/purl-spec for more information on purl
    public required string PUrl { get; init; }

    // An array of license IDs (SPDX codes)
    public required string[] Licenses { get; init; }

    public required BuildPackageComplianceInfo Compliance { get; init; }

    public required BuildPackageVulnerability Vulnerabilities { get; init; }
}
