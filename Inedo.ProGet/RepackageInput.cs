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

// JSON Object used by the Repackage Package HTTP endpoints

public sealed class RepackageInput
{
    // Name of the feed containing the package to repackage (e.g. "MyFeed")
    public required string Feed { get; init; }

    // Group/namespace of the package to repackage (e.g. "MyCompany.Library")
    public string? Group { get; init; }

    // Name of the package to repackage (e.g. "MyPackage")
    public required string Name { get; init; }

    // Current version of the package to repackage (e.g. "1.0.0")
    public required string Version { get; init; }

    // New version number for the repackaged package (e.g. "1.0.1")
    public required string NewVersion { get; init; }

    // Comments regarding the repackaging operation (e.g. "Fixed issue with dependencies")
    public string? Comments { get; init; }

    // Target feed to which the repackaged package will be published (e.g. "MyFeed-Repackaged")
    // * If not specified, the feed specified in the Feed property will be used
    public string? ToFeed { get; init; }
}
