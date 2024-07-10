using System.Text.Json.Serialization;

namespace Inedo.ProGet;

public sealed class BuildIssue
{
    public int Number { get; set; }
    public DateTime Created { get; set; }
    public string? Detail { get; set; }
    [JsonPropertyName("purl")]
    public required string PUrl { get; set; }
    public bool Resolved { get; set; }
}
