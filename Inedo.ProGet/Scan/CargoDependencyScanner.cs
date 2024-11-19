using System.Text.Json;
using Tomlyn;
using Tomlyn.Model;

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

        var cargoManifestFile = await this.FileSystem.FindFilesAsync(searchDirectory, "Cargo.toml", true, cancellationToken).FirstOrDefaultAsync() ?? throw new DependencyScannerException($"Cannot find Cargo.toml at {searchDirectory}");
        using var maniFestStream = await this.FileSystem.OpenReadAsync(cargoManifestFile.FullName, cancellationToken).ConfigureAwait(false);
        using var manifestReader = new StreamReader(maniFestStream);
        var cargoManifest = Toml.ToModel(await manifestReader.ReadToEndAsync(), cargoManifestFile.FullName);
        var name = cargoManifest.GetTable("package")?.GetNullableProperty<string>("name") ?? throw new DependencyScannerException("Cargo.toml missing package name.");

        await foreach (var cargoLockFile in this.FileSystem.FindFilesAsync(searchDirectory, "Cargo.lock", !this.SourcePath.EndsWith("Cargo.lock"), cancellationToken))
        {
            using var stream = await this.FileSystem.OpenReadAsync(cargoLockFile.FullName, cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            var cargoLock = Toml.ToModel(await reader.ReadToEndAsync(), cargoLockFile.FullName);

            projects.Add(new ScannedProject(name, ReadCargoLockFile(name, cargoLock).Distinct()));
        }

        return projects;
    }

    private IEnumerable<DependencyPackage> ReadCargoLockFile(string packageName, TomlTable doc)
    {
        foreach(var package in doc.GetTableArray("package") ?? [])
        {
            var name = package.GetProperty<string>("name");

            if (name.Equals(packageName))
                continue;

            var version = package.GetProperty<string>("version");
            yield return new DependencyPackage { Name = name, Version = version, Type = "cargo" };
        }
    }
}

public static class TomlExtensions
{
    public static TomlTable? GetTable(this TomlTable? table, string name) => table?.GetNullableProperty<TomlTable>(name);

    public static TomlTableArray? GetTableArray(this TomlTable? table, string name) => table?.GetNullableProperty<TomlTableArray>(name);

    public static T GetProperty<T>(this TomlTable table, string name) => (T)table[name];

    public static T? GetNullableProperty<T>(this TomlTable? table, string name) where T : class
    {
        if (table?.ContainsKey(name) ?? false)
            return table?[name] as T;
        return null;
    }
}