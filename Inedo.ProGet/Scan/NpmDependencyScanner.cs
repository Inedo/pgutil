using System.Text.Json;
using YamlDotNet.RepresentationModel;

namespace Inedo.DependencyScan;

internal sealed class NpmDependencyScanner(CreateDependencyScannerArgs args) : DependencyScanner(args)
{
    private readonly bool packageLockOnly = args.DoNotScanNodeModules;
    private readonly bool includeDevDependencies = args.IncludeDevDependencies;

    public override DependencyScannerType Type => DependencyScannerType.Npm;

    public override async Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default)
    {
        var projects = new List<ScannedProject>();

        // Handle pnpm lock file
        if (this.SourcePath.EndsWith("pnpm-lock.yaml"))
        {
            if(!await this.FileSystem.FileExistsAsync(this.SourcePath, cancellationToken))
                throw new FileNotFoundException("The specified pnpm lock file was not found.", this.SourcePath);
            using var stream = await this.FileSystem.OpenReadAsync(this.SourcePath, cancellationToken).ConfigureAwait(false);
            using var reader = new StreamReader(stream);

            var yaml = new YamlStream();
            yaml.Load(reader);

            var lockFileDocuments = yaml.Documents
                .Select(document => document.RootNode)
                .OfType<YamlMappingNode>()
                .ToList();

            if (lockFileDocuments.Count > 0)
            {
                // Try to get project name from package.json in the same directory
                var projectName = await GetProjectNameFromPackageJsonAsync(this.SourcePath, cancellationToken).ConfigureAwait(false);

                if (string.IsNullOrEmpty(projectName))
                    throw new InvalidOperationException($"Unable to determine project name from package.json in directory: {this.FileSystem.GetDirectoryName(this.SourcePath)}");
                
                var dependencies = lockFileDocuments
                    .Where(HasPackages)
                    .SelectMany(ReadPnpmLockFile)
                    .Distinct()
                    .ToList();
                projects.Add(new ScannedProject(projectName, dependencies));
            }

            return projects;
        }
        // Handle standard npm package-lock.json files
        else
        {
            var searchDirectory = (await this.FileSystem.FileExistsAsync(this.SourcePath, cancellationToken))
                            ? this.FileSystem.GetDirectoryName(this.SourcePath)
                            : this.SourcePath;

            if(await this.FileSystem.FindFilesAsync(searchDirectory, "pnpm-lock.yaml", true, cancellationToken).AnyAsync(cancellationToken: cancellationToken))
                Console.WriteLine("Warning: pnpm-lock.yaml file detected in the scan directory. To parse pNPM lock files, specify the the pnpm-lock.yaml file in the Source Path argument.");

            await foreach (var packageLockFile in this.FileSystem.FindFilesAsync(searchDirectory, "package-lock.json", !this.SourcePath.EndsWith("package-lock.json"), cancellationToken))
            {
                if (this.packageLockOnly && packageLockFile.FullName.Contains("node_modules", StringComparison.OrdinalIgnoreCase))
                    continue;

                using var stream = await this.FileSystem.OpenReadAsync(packageLockFile.FullName, cancellationToken).ConfigureAwait(false);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

                var projectName = doc.RootElement.GetProperty("name").GetString()!;
                projects.Add(new ScannedProject(projectName, ReadPackageLockFile(doc).Distinct()));
            }

            return projects;
        }

            
    }

    private static bool HasPackages(YamlMappingNode rootNode)
    {
        var packagesKey = new YamlScalarNode("packages");
        return rootNode.Children.TryGetValue(packagesKey, out var packagesNode)
            && packagesNode is YamlMappingNode;
    }

    private IEnumerable<DependencyPackage> ReadPackageLockFile(JsonDocument doc)
    {
        // works for file format version 2 & 3 (and probably later versions as well)
        if (doc.RootElement.TryGetProperty("packages", out var npmDependencyPackages))
            return ReadPackages(npmDependencyPackages);

        // legacy implementation for file format versions 1 & 2
        return ReadDependencies(doc.RootElement);
    }

    /// <summary>
    /// Read package-lock.json file format 2 and 3
    /// </summary>
    /// <param name="npmDependencyPackages"></param>
    /// <returns></returns>
    private IEnumerable<DependencyPackage> ReadPackages(JsonElement npmDependencyPackages)
    {
        foreach (var npmDependencyPackage in npmDependencyPackages.EnumerateObject())
        {
            // skip the self reference package
            if (npmDependencyPackage.Name.Equals(string.Empty))
                continue;

            string name;
            // drop the pre-pended paths, if they exist, to get the name of the package by itself
            var lidx = npmDependencyPackage.Name.LastIndexOf("node_modules/") + 13;
            if (lidx < 13 || lidx >= npmDependencyPackage.Name.Length)
                name = npmDependencyPackage.Name;
            else
                name = npmDependencyPackage.Name[lidx..];

            // check for package alias
            if (npmDependencyPackage.Value.TryGetProperty("name", out var alias) && alias.ValueKind == JsonValueKind.String)
                name = alias.GetString()!;

            if (!npmDependencyPackage.Value.TryGetProperty("version", out var versionProperty))
                continue;

            string version = versionProperty.GetString()!;

            var isDevDependency = npmDependencyPackage.Value.TryGetProperty("dev", out var dev) && dev.GetBoolean();

            if (isDevDependency && !this.includeDevDependencies)
                continue;

            // return dependency
            yield return CreateNpmDependencyPackage(name, version);
        }

    }

    /// <summary>
    /// Read package-lock.json file format 1 (and 2, which is backwards-compatible to 1)
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    private IEnumerable<DependencyPackage> ReadDependencies(JsonElement doc)
    {
        // for recursive calls: if for any reason npmDependencyPackage.Value is a primitive type instead of an object, yield nothing
        if (doc.ValueKind != JsonValueKind.Object)
            yield break;

        // get "dependencies" property
        if (doc.TryGetProperty("dependencies", out var npmDependencyPackages))
        {
            foreach (var npmDependencyPackage in npmDependencyPackages.EnumerateObject())
            {
                // skip the self reference package
                if (npmDependencyPackage.Name.Equals(string.Empty))
                    continue;

                string name = npmDependencyPackage.Name;

                string version = npmDependencyPackage.Value.GetProperty("version").GetString()!;

                // Check for npm package alias of format 'npm:package-name@package-version'
                if (version.StartsWith("npm:", StringComparison.OrdinalIgnoreCase) && version.Contains('@'))
                {
                    // If a npm package alias is used the information about the package is stored in the version-property
                    // The package name starts after 'npm:' and ends at the last occurence of '@'
                    // The package version comes after the last occurence of '@'
                    int separator = version.LastIndexOf('@');
                    name = version[..separator].Remove(0, 4);
                    version = version[(separator + 1)..];
                }

                var isDevDependency = npmDependencyPackage.Value.TryGetProperty("dev", out var dev) && dev.GetBoolean();

                if (isDevDependency && !this.includeDevDependencies)
                    continue;

                // return dependency
                yield return CreateNpmDependencyPackage(name, version);

                // check for sub-dependencies recursively
                foreach (var subDependency in ReadDependencies(npmDependencyPackage.Value))
                    yield return subDependency;
            }
        }
    }

    /// <summary>
    /// Extrac the package name from the package.json file
    /// </summary>
    /// <param name="pnpmLockPath">pnpm package lock file path</param>
    /// <param name="cancellationToken">Cancellation Tocken</param>
    /// <returns>npm Project Name</returns>
    /// <remarks>This should only be used when parsing pnpm lock files.  pnpm does not store the package name in the lock file like npm does.</remarks>
    private async Task<string?> GetProjectNameFromPackageJsonAsync(string pnpmLockPath, CancellationToken cancellationToken)
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

    /// <summary>
    /// Read the pnpm lock file (pnpm-lock.yaml) file format for packages
    /// </summary>
    /// <param name="rootNode">Root Yaml Mapping Node</param>
    /// <returns>A list of dependency pagkcages</returns>
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

    /// <summary>
    /// Parse the package key to extract name and version
    /// </summary>
    /// <param name="packageKey">pnpm package key</param>
    /// <returns></returns>
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
