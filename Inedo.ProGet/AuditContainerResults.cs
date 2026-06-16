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

// JSON Object used by the Audit Container HTTP endpoint
public sealed class AuditContainerResults
{
    // Name of the Docker repository of the image being audited
    public required string Repository { get; init; }

    // Tags for this image in the repository
    public string[]? Tags { get; init; }

    // Unique digest of the image
    public required string Digest { get; init; }

    // Timestamp when the image was added to ProGet
    public required DateTime Created { get; init; }

    // PUrls of packages used by the image
    public string[]? Packages { get; init; }

    // Known vulnerabilities in packages in the image
    public VulnerabilityInfo[]? Vulnerabilities { get; init; }

    // If image is a manifest list (fat manifest), then this is a list of the subimages. Otherwise, it is null
    public ContainerManifest[]? ManifestList { get; init; }
}
