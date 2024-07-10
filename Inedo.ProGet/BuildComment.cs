namespace Inedo.ProGet;

public sealed class BuildComment
{
    public int Number { get; set; }
    public string? By { get; set; }
    public DateTime Date { get; set; }
    public string? Comment { get; set; }
}
