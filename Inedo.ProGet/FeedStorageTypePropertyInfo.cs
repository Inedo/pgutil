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

// JSON Object returned as part of FeedStorageType.cs by the List Feed Storage Types HTTP endpoint
public sealed class FeedStorageTypePropertyInfo
{
    // Indicates if the property is required when configuring the storage type (e.g. "true" or "false")
    public bool Required { get; init; }

    // Name of the property (e.g. "Storage path")
    public string? Name { get; init; }

    // Description of the property (e.g. "Local file system path or network share.")
    public string? Description { get; init; }

    // Type of the property if applicable (e.g. "boolean")
    public string? Type { get; init; }

    // Placeholder text for the property (e.g. "managed by ProGet", "none (use bucket root)")
    public string? Placeholder { get; init; }
}
