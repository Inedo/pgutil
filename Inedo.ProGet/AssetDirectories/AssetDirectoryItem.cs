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

namespace Inedo.ProGet.AssetDirectories;

// JSON Object used by the List Asset Folders HTTP endpoint
public sealed class AssetDirectoryItem
{
    // The name of the Asset item (File or Folder)
    public required string Name { get; init; }

    // The full path of the parent directory of the item
    // * Omitted if the item is contained in the root folder of the asset directory
    public string? Parent { get; init; }

    // The number of bytes in size of the item.
    public long? Size { get; init; }

    // Either the Content-Type of the the item, or "dir" if the item represents a folder.
    public required string Type { get; init; }

    // The full URL of the item.
    public string? Content { get; init; }

    // The UTC date of the original creation time of the item
    public DateTime Created { get; init; }

    // The UTC date of the last time of the item was updated
    public DateTime Modified { get; init; }

    // The MD5 hash of the item in hexadecimal format
    [JsonPropertyName("md5")]
    public string? MD5 { get; init; }

    // The SHA1 hash of the item in hexadecimal format
    [JsonPropertyName("sha1")]
    public string? SHA1 { get; init; }

    // The SHA256 hash of the item in hexadecimal format
    [JsonPropertyName("sha256")]
    public string? SHA256 { get; init; }

    // The SHA512 hash of the item in hexadecimal format
    [JsonPropertyName("sha512")]
    public string? SHA512 { get; init; }

    public Dictionary<string, AssetUserMetadata>? UserMetadata { get; init; }

    public AssetDirectoryItemCacheHeader? CacheHeader { get; init; }

    // this property is *not* in the JSON Object; it's used as a helper property Inedo.ProGet library
    public bool Directory => this.Type == "dir";
}

// AssetDirectoryItem may return additional properties from an HTTP request:
// * sha1 - contains the SHA1 hash of the item.
