namespace Inedo.ProGet;

public sealed class BuildCommentCreateInfo
{
    public required string Project { get; init; }
    public required string Version { get; init; }
    public int? Number { get; set; }
    public string? By { get; set; }
    public DateTime? Date { get; set; }
    public string? Comment { get; set; }
}
