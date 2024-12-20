using System.Text.Json;
using Inedo.DependencyScan;

namespace Inedo.ProGet.Scan;
internal class ComposerDependencyScanner(CreateDependencyScannerArgs args) : DependencyScanner(args)
{
    public override DependencyScannerType Type => DependencyScannerType.Composer;

    public override async Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default)
    {
        var projects = new List<ScannedProject>();
        var searchDirectory = (await this.FileSystem.FileExistsAsync(this.SourcePath, cancellationToken))
            ? this.FileSystem.GetDirectoryName(this.SourcePath)
            : this.SourcePath;

        var composerManifestFile = await this.FileSystem.FindFilesAsync(searchDirectory, "composer.json", true, cancellationToken).FirstOrDefaultAsync() ?? throw new DependencyScannerException($"Cannot find composer.json at {searchDirectory}");
        using var maniFestStream = await this.FileSystem.OpenReadAsync(composerManifestFile.FullName, cancellationToken).ConfigureAwait(false);
        using var manifestDoc = await JsonDocument.ParseAsync(maniFestStream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!manifestDoc.RootElement.TryGetProperty("name", out var projectName))
            throw new DependencyScannerException($"composer.json at {searchDirectory} does not contain a name");

        var composerLockFile = await this.FileSystem.FindFilesAsync(searchDirectory, "composer.lock", true, cancellationToken).FirstOrDefaultAsync() ?? throw new DependencyScannerException($"Cannot find composer.lock at {searchDirectory}");
        using var lockFileStream = await this.FileSystem.OpenReadAsync(composerLockFile.FullName, cancellationToken).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(lockFileStream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if(!doc.RootElement.TryGetProperty("packages", out var packages) && packages.ValueKind != JsonValueKind.Array)
            throw new DependencyScannerException($"composer.lock at {searchDirectory} does not contain any packages");

        var dependencies = new List<DependencyPackage>();
        dependencies.AddRange(readDependencies(packages));
        
        if(this.CreateArgs.IncludeDevDependencies && doc.RootElement.TryGetProperty("packages-dev", out var devPackages) && devPackages.ValueKind == JsonValueKind.Array)
            dependencies.AddRange(readDependencies(devPackages));

        projects.Add(new ScannedProject(projectName.GetString()!, dependencies.Distinct()));

        return projects;

        static IEnumerable<DependencyPackage> readDependencies(JsonElement packages)
        {
            foreach (var package in packages.EnumerateArray())
            {
                if (package.ValueKind != JsonValueKind.Object)
                    continue;

                var name = package.TryGetProperty("name", out var packageName) ? packageName.GetString() : null;
                var version = package.TryGetProperty("version", out var packageVersion) ? packageVersion.GetString() : null;
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(version))
                    continue;
                var fullName = name.Split('/', 2);
                yield return new DependencyPackage { Group = fullName[0], Name = fullName[1], Type = "composer", Version = version };
            }
        }
    }
}
