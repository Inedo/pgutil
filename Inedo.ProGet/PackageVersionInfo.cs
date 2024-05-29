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

using System.Text.Json.Serialization;

namespace Inedo.ProGet;

// JSON Object used by Packages List Latest/Versions HTTP endpoints
public sealed class PackageVersionInfo
{
    // PUrl of the package (see https://github.com/package-url/purl-spec/blob/master/PURL-TYPES.rst)
    [JsonPropertyName("purl")]
    public required string PUrl { get; init; }
    // Group of the package identifier if the package type supports groups
    public string? Group { get; init; }
    // Unique name of the package
    public required string Name { get; init; }
    // Version of the package
    public required string Version { get; init; }
    // Additional fields specific to the type of package (see https://github.com/package-url/purl-spec/blob/master/PURL-TYPES.rst)
    public string? Qualifier { get; init; }
    // Total number of downloads of all versions of the package from the ProGet feed
    public long TotalDownloads { get; init; }
    // Number of downloads of the latest version of the package from the ProGet feed
    public long Downloads { get; init; }
    // Timestamp when the package was published to ProGet
    public DateTime Published { get; init; }
    // User which published the package to ProGet; this information may not always be available
    public string? PublishedBy { get; init; }
    // Size of the package file in bytes
    public long Size { get; init; }
    // Indicates whether the package is visible to searches (assume true when not specified)
    public bool Listed { get; init; } = true;

    // Hashes are all in hexadecimal format
    [JsonPropertyName("md5")]
    public string? MD5 { get; init; }
    [JsonPropertyName("sha1")]
    public string? SHA1 { get; init; }
    [JsonPropertyName("sha256")]
    public string? SHA256 { get; init; }
    [JsonPropertyName("sha512")]
    public string? SHA512 { get; init; }

    // Indicates the allow download override value in ProGet for the package
    //  * when true, the package may always be downloaded regardless of any other rules
    //  * when false, the package may never be downloaded for any reason
    //  * when not specified, use normal package/compliance rules to determine if the package can be downloaded
    public bool? AllowDownload { get; init; }

    // Indicates if the package has been marked as deprecated
    public bool Deprecated { get; init; }

    // When deprecated is true, this may contain a reason for the package's deprecation (but this is not required)
    public string? DeprecationReason { get; init; }
}
