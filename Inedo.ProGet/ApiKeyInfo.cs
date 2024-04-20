namespace Inedo.ProGet;

public sealed class ApiKeyInfo
{
    public int? Id { get; init; }
    public ApiKeyType? Type { get; set; }
    public string? Key { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string[]? Apis { get; set; }
    public DateTime? Expiration { get; set; }
    public ApiKeyBodyLogging? Logging { get; set; }
    public ApiKeyConfigFeedTask[]? FeedPermissions { get; set; }
    public string? User { get; set; }
}

public enum ApiKeyType
{
    System,
    Feed,
    Personal,
    Other
}

public enum ApiKeyBodyLogging
{
    None,
    Request,
    Response,
    Both
}

public sealed record class ApiKeyConfigFeedTask(string? Feed, string? FeedGroup, string Permission);
