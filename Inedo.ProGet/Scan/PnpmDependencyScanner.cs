using YamlDotNet.RepresentationModel;

namespace Inedo.DependencyScan;

internal sealed class PnpmDependencyScanner(CreateDependencyScannerArgs args) : DependencyScanner(args)
{
    private readonly bool includeDevDependencies = args.IncludeDevDependencies;

    public override DependencyScannerType Type => DependencyScannerType.Pnpm;

    public override async Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default)
    {
        var projects = new List<ScannedProject>();
        var searchDirectory = (await this.FileSystem.FileExistsAsync(this.SourcePath, cancellationToken))
            ? this.FileSystem.GetDirectoryName(this.SourcePath)
            : this.SourcePath;

        await foreach (var pnpmLockFile in this.FileSystem.FindFilesAsync(searchDirectory, "pnpm-lock.yaml", !this.SourcePath.EndsWith("pnpm-lock.yaml"), cancellationToken))
        {
            using var stream = await this.FileSystem.OpenReadAsync(pnpmLockFile.FullName, cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);
            
            var yaml = new YamlStream();
            yaml.Load(reader);

            if (yaml.Documents.Count > 0 && yaml.Documents[0].RootNode is YamlMappingNode rootNode)
            {
                // Try to get project name from package.json in the same directory
                var projectName = await GetProjectNameAsync(pnpmLockFile.FullName, cancellationToken).ConfigureAwait(false);
                
                if (string.IsNullOrEmpty(projectName))
                {
                    throw new InvalidOperationException($"Unable to determine project name from package.json in directory: {this.FileSystem.GetDirectoryName(pnpmLockFile.FullName)}");
                }
                
                var dependencies = ReadPnpmLockFile(rootNode).ToList();
                projects.Add(new ScannedProject(projectName, dependencies));
            }
        }

        return projects;
    }

    private async Task<string?> GetProjectNameAsync(string pnpmLockPath, CancellationToken cancellationToken)
    {
        try
        {
            var directory = this.FileSystem.GetDirectoryName(pnpmLockPath);
            var packageJsonPath = Path.Combine(directory, "package.json");
            
            if (await this.FileSystem.FileExistsAsync(packageJsonPath, cancellationToken).ConfigureAwait(false))
            {
                using var stream = await this.FileSystem.OpenReadAsync(packageJsonPath, cancellationToken).ConfigureAwait(false);
                using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
                
                if (doc.RootElement.TryGetProperty("name", out var nameProperty))
                {
                    return nameProperty.GetString();
                }
            }
        }
        catch
        {
            // Ignore errors when reading package.json
            // Later a meaningful exception will be thrown, when the project name is empty
        }

        return null;
    }

    private IEnumerable<DependencyPackage> ReadPnpmLockFile(YamlMappingNode rootNode)
    {
        // Find the "packages" node
        var packagesKey = new YamlScalarNode("packages");
        if (!rootNode.Children.TryGetValue(packagesKey, out var packagesNode) || packagesNode is not YamlMappingNode packagesMapping)
            yield break;

        foreach (var package in packagesMapping.Children)
        {
            if (package.Key is not YamlScalarNode keyNode || package.Value is not YamlMappingNode valueNode)
                continue;

            // Package key format in pnpm-lock.yaml is typically:
            // - "/@scope/package@version" or "/package@version" for regular dependencies
            // - "/@scope/package@version(peer-deps)" for packages with peer dependencies
            
            var packageKey = keyNode.Value;
            
            // Skip root package
            if (string.IsNullOrEmpty(packageKey) || packageKey == ".")
                continue;

            // Remove leading slash if present
            if (packageKey.StartsWith('/'))
                packageKey = packageKey[1..];

            // Check if it's a dev dependency
            var devKey = new YamlScalarNode("dev");
            if (valueNode.Children.TryGetValue(devKey, out var devNode) && 
                devNode is YamlScalarNode devScalar && 
                bool.TryParse(devScalar.Value, out var isDev) && 
                isDev && !this.includeDevDependencies)
                continue;

            // Parse package name and version
            var (name, version) = ParsePackageKey(packageKey);
            
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(version))
            {
                yield return CreateNpmDependencyPackage(name, version);
            }
        }
    }

    private static (string name, string version) ParsePackageKey(string packageKey)
    {
        // Remove anything in parentheses (peer dependency info)
        var parenIndex = packageKey.IndexOf('(');
        if (parenIndex >= 0)
            packageKey = packageKey[..parenIndex];

        // Find the last @ symbol which separates name from version
        // For scoped packages like @scope/package@1.0.0, we need to find the @ after the scope
        var lastAtIndex = packageKey.LastIndexOf('@');
        
        if (lastAtIndex <= 0) // No version found or @ is at the start (scoped package without version)
            return (packageKey, string.Empty);

        // For scoped packages, make sure we don't split on the first @
        if (packageKey.StartsWith('@'))
        {
            // This is a scoped package
            var secondAtIndex = packageKey.IndexOf('@', 1);
            if (secondAtIndex > 0)
                lastAtIndex = secondAtIndex;
        }

        var name = packageKey[..lastAtIndex];
        var version = packageKey[(lastAtIndex + 1)..];

        return (name, version);
    }

    private static DependencyPackage CreateNpmDependencyPackage(string name, string version)
    {
        // Check for scoped name ("@scopename/packagename")
        string? group = null;
        var parts = name.Split('/', 2);
        if (name.StartsWith('@') && parts.Length == 2)
        {
            // group = scope name
            group = parts[0];
            // name = package name
            name = parts[1];
        }

        return new DependencyPackage { Group = group, Name = name, Version = version, Type = "npm" };
    }
}
