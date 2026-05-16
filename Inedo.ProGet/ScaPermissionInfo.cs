namespace Inedo.ProGet;

public sealed class ScaPermissionInfo
{
    public bool CanView { get; init; }
    public bool CanManage { get; init; }
    public bool CanUploadSbom { get; init; }
}
