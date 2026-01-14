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

// JSON Object used by BuildInfo.cs
public sealed class BuildCommentCreateInfo
{
    // Project name associated with the build (e.g. "My Application")
    public required string Project { get; init; }

    // Build version associated with the build (e.g. "1.0.0")
    public required string Version { get; init; }

    // Internal comment ID number (e.g. 2)
    public int? Number { get; set; }

    // Name of the user that created the comment (e.g. "jsmith")
    public string? By { get; set; }

    // Date that the comment was created
    // * Date is output in ISO8601 format (e.g. "2019-08-01T00:00:00-07:00")
    public DateTime? Date { get; set; }

    // The comment text (e.g. "This issue has been resolved.")
    public string? Comment { get; set; }
}
