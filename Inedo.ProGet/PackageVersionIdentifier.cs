namespace Inedo.ProGet;

public sealed record class PackageVersionIdentifier(

    string Type,

    string Name,

    string Version,

    string? Group = null
);
