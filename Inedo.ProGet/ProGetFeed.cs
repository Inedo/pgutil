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

// JSON Object used by Feeds Create/Get/List/Update HTTP endpoints
public sealed class ProGetFeed
{
    // The unique name of the feed
    // * Must not include characters that require URL escaping
    public string? Name { get; set; }
    public string[]? AlternateNames { get; set; }

    // The type of package feed
    // * Valid values: universal, nuget, chocolatey, npm, maven, powershell, 
    // * docker, rubygems, vsix, asset, romp, pypi, helm, rpm, conda, cran
    public string? FeedType { get; set; }

    // A description displayed with the ProGet feed name in the UI
    // * Supplying "null" removes the description
    public string? Description { get; set; }

    // Indicates whether the feed is active ("true") or disabled ("false")
    public bool? Active { get; set; }

    // Indicates whether connector caching is enabled for the feed
    public bool? CacheConnectors { get; set; }

    // Disk path accessible to ProGet's service where packages may be added to the feed
    // * Supplying "null" disables the drop path
    public string? DropPath { get; set; }

    // Absolute disk path for package storage if a "packageStore" is not specified
    // * Supplying "null" will use a path relative to the value of the "Storage.PackagesRootPath" setting
    public string? PackagesPath { get; set; }

    // Serialized configuration for a custom package store such as Amazon S3 or Azure Blob
    // * Supplying "null" will use ProGet-managed storage as specified by the "packagesPath"
    public string? PackageStore { get; set; }

    // Indicates whether packages with unknown licenses are allowed
    // * Supplying "null" will will inherit the value of the "Feeds.AllowUnknownLicenseDownloads" setting
    public bool? AllowUnknownLicenses { get; set; }

    // Array of SPDX license identifiers (e.g. MIT) known to ProGet allowed to be downloaded from the feed
    // * Supply an empty array to remove them all
    public string[]? AllowedLicenses { get; set; }

    // Array of SPDX license identifiers known to ProGet blocked from being downloaded from the feed
    // * Supply an empty array to remove them all
    public string[]? BlockedLicenses { get; set; }

    // Indicates whether the NuGet symbol server is enabled; only applies to NuGet-like feed types
    // * NuGet packages only
    public bool? SymbolServerEnabled { get; set; }

    // Indicates whether symbol files (i.e., .pdb files) are removed from a NuGet-like package when downloaded
    // * NuGet packages only
    public bool? StripSymbols { get; set; }

    // Indicates signature files should be stripped on NuGet packages when downloaded
    // * NuGet packages only
    // * Requires ProGet 2022.27 or later
    public bool? StripSignature { get; set; }

    // Indicates whether the NuGet v3 API should be used on NuGet feeds
    // * NuGet packages only
    // * Requires ProGet 2022.27 or later
    public bool? UseApiV3 { get; set; }

    public string? EndpointUrl { get; set; }

    // Array of connectors for the feed
    // * Supply an empty array to remove all feed connectors
    public string[]? Connectors { get; set; }

    // Array of vulnerability sources for the feed
    // * Supply an empty array to remove all vulnerability sources
    public string[]? VulnerabilitySources { get; set; }

    // Array of "RetentionRule" objects that define the retention rules for the feed
    // Refer to RetentionRule.cs for more information
    // * Supply an empty array to remove all retention rules from the feed
    public RetentionRule[]? RetentionRules { get; set; }

    // Array of serialized configurations that define the custom package filters
    // * Supply an empty array to remove all package filters
    public Dictionary<string, string>? PackageFilters { get; set; }

    // Array of serialized configurations that define the custom package access rules
    // * Supply an empty array to remove all package access rules
    public Dictionary<string, string>? PackageAccessRules { get; set; }

    // An object whose property names represent variable names, and whose property values represent the variable value
    // * Supply an object with no properties to remove all variables.
    // * Naming rules: A variable name is a string of no more than fifty characters: 
    //      * numbers (0-9), upper- and lower-case letters (a-Z), dashes (-), spaces ( ), and underscores (_) and must start with a letter
    //      * May not start or end with a hyphen, underscore, or space
    //      * A variable value is a string of any number of characters
    public Dictionary<string, string>? VariableDictionary { get; set; }
    public bool? CanPublish { get; set; }

    //The following require ProGet 2022.26 or later:

    // Indicates whether individual downloads for advanced statistics should be recorded
    public bool? PackageStatisticsEnabled { get; set; }

    // Indicates whether viewing download statistics are restricted to Feed Administrators
    public bool? RestrictPackageStatistics { get; set; }

    // Indicates whether packages deployment records should be recorded
    public bool? DeploymentRecordsEnabled { get; set; }

    // Indicates whether usage records should be enabled
    public bool? UsageRecordsEnabled { get; set; }

    // Indicates whether vulnerability information should be displayed and download blocking rules should be enforced
    // * Not supported on all feed types
    public bool? VulnerabilitiesEnabled { get; set; }

    // Indicates whether license information should be displayed and download blocking rules should be enforced
    // * Not supported on all feed types
    public bool? LicensesEnabled { get; set; }

    // Indicates whether package usage in "Releases & Builds (SCA)" should be displayed
    public bool? UseWithProjects { get; set; }

    // Indicates whether retention rules are enabled on the feed
    // * Requires ProGet 2023.25 or later
    public bool? RetentionRulesEnabled { get; set; }
}

// ProGetFeed may return additional properties from an HTTP request:
// * stripSource (NuGet Packages Only): indicates whether source files (i.e., files under `src/` in the package) are removed from a NuGet-like package when downloaded