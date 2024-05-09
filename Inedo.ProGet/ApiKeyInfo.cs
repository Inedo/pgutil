using System.Text.Json.Serialization;
using System;

namespace Inedo.ProGet;

public sealed class ApiKeyInfo
{
    public readonly static string[] AvailablePackagePermissions = ["view","add","promote","delete"];
    public int? Id { get; set; }
    public ApiKeyType? Type { get; set; }
    public string? Key { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string[]? SystemApis { get; set; }
    public string[]? PackagePermissions { get; set; }
    public string? Feed { get; set; }
    public string? FeedGroup { get; set; }
    public DateTime? Expiration { get; set; }
    public ApiKeyBodyLogging? Logging { get; set; }
    public string? User { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<ApiKeyType>))]
public enum ApiKeyType
{
    System,
    Feed,
    Personal,
    Other
}

[JsonConverter(typeof(JsonStringEnumConverter<ApiKeyBodyLogging>))]
public enum ApiKeyBodyLogging
{
    None,
    Request,
    Response,
    Both
}
