namespace Inedo.DependencyScan;

public sealed record class CreateDependencyScannerArgs(
    string SourcePath,
    ISourceFileSystem FileSystem,
    bool PackageLockOnly = false,
    bool IncludeDevDependencies = false,
    IReadOnlyList<string>? IncludeFolders = null,
    bool IncludeProjectReferences = false
);
