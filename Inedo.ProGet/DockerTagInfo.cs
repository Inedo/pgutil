namespace Inedo.ProGet;

public sealed class DockerTagInfo
{
    public required string Repository { get; set; }
    public required string Digest { get; set; }
    public string? Tag { get; set; }
    public DateTime? Published { get; set; }
    public string? PublishedBy { get; set; }
    public int TotalDownloads { get; set; }
    public long DownloadSize { get; set; }
}
