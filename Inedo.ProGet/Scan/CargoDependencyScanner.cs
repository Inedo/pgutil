using Tomlyn;

namespace Inedo.DependencyScan;

internal sealed class CargoDependencyScanner(CreateDependencyScannerArgs args) : DependencyScanner(args)
{
    public override DependencyScannerType Type => DependencyScannerType.Cargo;

    public override async Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default)
    {
        var projects = new List<ScannedProject>();
        var searchDirectory = (await this.FileSystem.FileExistsAsync(this.SourcePath, cancellationToken))
            ? this.FileSystem.GetDirectoryName(this.SourcePath)
            : this.SourcePath;

        var cargoManifestFile = await this.FileSystem.FindFilesAsync(searchDirectory, "Cargo.toml", true, cancellationToken).FirstOrDefaultAsync(cancellationToken) ?? throw new DependencyScannerException($"Cannot find Cargo.toml at {searchDirectory}");
        using var manifestStream = await this.FileSystem.OpenReadAsync(cargoManifestFile.FullName, cancellationToken).ConfigureAwait(false);
        var cargoManifest = TomlSerializer.Deserialize(manifestStream, CargoTomlContext.Default.CargoManifest)!;
        var name = cargoManifest.Package.Name;

        await foreach (var cargoLockFile in this.FileSystem.FindFilesAsync(searchDirectory, "Cargo.lock", !this.SourcePath.EndsWith("Cargo.lock"), cancellationToken))
        {
            using var stream = await this.FileSystem.OpenReadAsync(cargoLockFile.FullName, cancellationToken).ConfigureAwait(false);
            projects.Add(new ScannedProject(name, ReadCargoLockFile(name, stream).Distinct()));
        }

        return projects;
    }

    private static IEnumerable<DependencyPackage> ReadCargoLockFile(string packageName, Stream lockFileStream)
    {
        var lockFile = TomlSerializer.Deserialize(lockFileStream, CargoTomlContext.Default.CargoLockFile);
        if (lockFile?.Package is not null)
        {
            foreach (var package in lockFile.Package)
            {
                if (string.IsNullOrEmpty(package.Name) || package.Name == packageName || string.IsNullOrEmpty(package.Version))
                    continue;

                yield return new DependencyPackage { Name = package.Name, Version = package.Version, Type = "cargo" };
            }
        }
    }
}
