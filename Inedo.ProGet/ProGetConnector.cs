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

namespace Inedo.ProGet;

// JSON Object used by Connectors Create/Get/List/Update HTTP endpoints
public sealed class ProGetConnector
{
    // The unique name of the connector (Max 100 characters)
    // * Must not include characters that require URL escaping
    public required string Name { get; set; }

    // The URL of the connector
    public required string Url { get; set; }

    // The type of package feed
    // * Valid values: universal, nuget, chocolatey, npm, maven, powershell, 
    // * docker, rubygems, vsix, asset, romp, pypi, helm, rpm, conda, cran
    public required string FeedType { get; set; }

    // The timeout (in seconds) used when making a request to the connector URL
    public int? Timeout { get; set; }

    // Indicates whether metadata caching is enabled on the connector
    public bool? MetadataCacheEnabled { get; set; }

    // * The number of minutes a connector metadata request to a specific URL is cached by ProGet 
    // * Default value: `30`
    public int? MetadataCacheMinutes { get; set; }

    // the number of URL-specific metadata requests cached by ProGet
    // * Default value: `100`
    public int? MetadataCacheCount { get; set; }

    // The username used for authentication at the connector URL
    public string? Username { get; set; }

    // The password used for authentication at the connector URL
    public string? Password { get; set; }

    // An array of connector filters
    // * Supply an empty array to remove all filters; 
    // * Supports [wildcards and negations]
    // * Adding a filter will give it the default `Allow` behavior. 
    // * To add a filter with a `Block` behavior, use a `!` prefix (e.g. `!filter`)
    public string[]? Filters { get; set; }
}
