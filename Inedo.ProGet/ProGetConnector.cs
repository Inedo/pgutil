namespace Inedo.ProGet;

public sealed class ProGetConnector
{
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string FeedType { get; set; }
    public int? Timeout { get; set; }
    public bool? MetadataCacheEnabled { get; set; }
    public int? MetadataCacheMinutes { get; set; }
    public int? MetadataCacheCount { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string[]? Filters { get; set; }
}
