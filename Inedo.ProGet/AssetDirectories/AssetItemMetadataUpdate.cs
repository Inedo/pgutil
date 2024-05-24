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

namespace Inedo.ProGet.AssetDirectories;

// JSON Object used by the Update Asset Metadata HTTP endpoint
internal sealed class AssetItemMetadataUpdate
{
    // Contains the new Content-Type of the the Asset item. 
    // * Ignored for directories. If not specified, the Content-Type is not updated.
    public string? Type { get; init; }

    // Contains one of: 
    // * "update" (create/update properties)
    // * "replace" (create/update properties and delete missing values)
    public string? UserMetadataUpdateMode { get; init; }

    // updated key/value pairs of user-defined metadata for the item.
    public IReadOnlyDictionary<string, AssetUserMetadata>? UserMetadata { get; init; }
}

// HTTP Endpoint may additional properties in the JSON object:
// * cacheHeader - Contains two properties, type and value.
