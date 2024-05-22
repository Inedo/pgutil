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

// JSON Object used by the ProGet Health HTTP endpoint
public sealed class ProGetHealthInfo
{
    // The name of the Inedo application
    // * For this API it will be ProGet as default
    public required string ApplicationName { get; init; }

    // The health state of the database.
    // * Value is either "OK" or "Error"
    public required string DatabaseStatus { get; init; }

    // A specific error message if DatabaseStatus is "Error"
    // * The default value is "null"
    public string? DatabaseStatusDetails { get; init; }

    // The health state of the product license.
    // * Value is either "OK" or "Error"
    public required string LicenseStatus { get; init; }

    // A specific error message if LicenseStatus is "Error"
    // * The default value is "null"
    public string? LicenseStatusDetail { get; init; }

    // The current version number of the instance
    public required string VersionNumber { get; init; }

    // The current release number of the instance
    public required string ReleaseNumber { get; init; }

    // The health state of the service.
    // * Value is either "OK" or "Error"
    public required string ServiceStatus { get; init; }

    // A specific error message if ServiceStatus is "Error"
    // * The default value is "null"
    public string? ServiceStatusDetail { get; init; }

    // A ReplicationStatusInfo Object
    public ReplicationStatusInfo? ReplicationStatus { get; init; }

    // A JSON Object used by the ProGet Health HTTP endpoint
    // A subset of the ProGetHealthInfo Object
    public sealed class ReplicationStatusInfo
    {
        // The health state of a replication server 
        // * Value is either "OK" or "Error" if there are existing replication servers.
        // * Value is "null" if there are no replication servers.
        public string? ServerStatus { get; init; }
        // A specific error message if ServerStatus is "Error"
        // * The default value is "null"
        public string? ServerError { get; init; }
        // The health state of a replication client 
        // * Value is either "OK" or "Error" if there are existing replication clients.
        // * Value is "null" if there are no replication clients.
        public string? ClientStatus { get; init; }
        // A specific error message if ClientStatus is "Error"
        // * The default value is "null"
        public string? ClientError { get; init; }
    }
}