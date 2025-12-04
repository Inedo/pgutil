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

namespace Inedo.ProGet;

// JSON Object used by the Add Permission HTTP endpoint
public sealed class SecurityPermission
{
    // Unique id number of the permission (e.g. 3)
    public int? Id { get; init; }

    // User granted/denied the permission (e.g. "jsmith")
    public string? User { get; init; }

    // Group granted/denied the permission (e.g. "Developers")
    public string? Group { get; init; }

    // Task associated with the permission (e.g. "View & Download Packages")
    public string? Task { get; init; }

    // Feed that the permission is scoped to (e.g. "internal-nuget")
    public string? Feed { get; init; }

    // Feed group that the permission is scoped to (e.g. "nuget-feeds")
    public string? FeedGroup { get; init; }

    // Indicates if the permission is denied ("true") or granted ("false")
    public bool Deny { get; init; }
}