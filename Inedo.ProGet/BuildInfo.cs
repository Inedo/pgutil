﻿/*******************************************************************************
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

// JSON Object used by the Create/Update Build HTTP endpoint
public sealed class BuildInfo
{
    // The build version number
    public required string Version { get; init; }
    
    // Indicates whether the build is "active" or "inactive"
    public bool Active { get; init; }

    // SBOM Metadata field
    public string? Url { get; init; }

    // An absolute URL for the build overview page
    public required string ViewBuildUrl { get; init; }

    // Timestamp when the build was created
    public DateTime? Created { get; init; }

    // Release number field
    public string? Release { get; init; }

    // Current stage of the build
    public string? Stage { get; init; }

    public BuildComment[]? Comments { get; init; }

    public BuildPackage[]? Packages { get; init; }
}
