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
// * Status property values will be either "OK" or "Error"
// * StatusDetails properties will return an error message if Status is "Error", else will be null
public sealed class ProGetHealthInfo
{
    // The health state of the database (e.g. "OK")
    public required string DatabaseStatus { get; init; }

    // Details about the database status. ("null" if DatabaseStatus is "OK")
    public string? DatabaseStatusDetails { get; init; }

    // The health state of the product license. (e.g. "OK")
    public required string LicenseStatus { get; init; }

    // Details about the product license status. ("null" if LicenseStatus is "OK")
    public string? LicenseStatusDetail { get; init; }

    // The current version number of ProGet (e.g. "2025.18")
    public required string VersionNumber { get; init; }

    // The current release number of ProGet (e.g. "2025.0.18")
    public required string ReleaseNumber { get; init; }

    // The health state of the service. (e.g. "OK")
    public required string ServiceStatus { get; init; }

    // Details about the service status. ("null" if ServiceStatus is "OK")
    public string? ServiceStatusDetail { get; init; }

    // Information on status of replication (if configured, else will be null).
    public ReplicationStatusInfo? ReplicationStatus { get; init; }
    
    // JSON Object used by ReplicationStatus property
    // * Status property values will be either "OK" or "Error" if replication servers exist, else will be null
    // * Error properties will return an error message if Status is "Error", else will be null
    public sealed class ReplicationStatusInfo
    {
        // The health state of a replication server if applicable, else will be null
        public string? ServerStatus { get; init; }

        // The error message for a replication server, else will be null
        public string? ServerError { get; init; }

        // The health state of a replication client if applicable, else will be null
        public string? ClientStatus { get; init; }

        // The error message for a replication client, else will be null
        public string? ClientError { get; init; }
    }
}

// ProGet Health HTTP endpoint may return additional properties on the JSON object
// * applicationName - Will always be "ProGet"
// * extensionsInstalled - Describes extensions installed in the instance and their versions (e.g. "Azure": "2.0.1")
// * replicationStatus object may have additional properties:
//   * incoming - Duplicates and provides information about any existing Replications
//   * outgoing - Duplicates and provides information about any existing Replications