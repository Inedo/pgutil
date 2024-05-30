namespace Inedo.ProGet;

public sealed record class PackageStatus(bool? Listed = null, bool? Allow = null, bool? Deprecated = null, string? DeprecationReason = null);
