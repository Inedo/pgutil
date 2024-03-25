using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Inedo.DependencyScan;

internal sealed partial class NuGetDependencyScanner(CreateDependencyScannerArgs args) : DependencyScanner(args)
{
    private readonly HashSet<string> includeFolders = [.. args.IncludeFolders ?? []];
    private readonly bool considerProjectReferences = args.IncludeProjectReferences;

    public override DependencyScannerType Type => DependencyScannerType.NuGet;

    public override async Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default)
    {
        if (this.SourcePath.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            var projects = new List<ScannedProject>();

            var solutionRoot = this.FileSystem.GetDirectoryName(this.SourcePath);

            var assets = await this.FindAllAssetsAsync(solutionRoot, cancellationToken).ConfigureAwait(false);

            await foreach (var p in ReadFoldersAndProjectsFromSolutionAsync(this.SourcePath, cancellationToken).ConfigureAwait(false))
            {
                var projectPath = this.FileSystem.Combine(solutionRoot, p);
                IAsyncEnumerable<DependencyPackage> packages;

                if (assets.TryGetValue(projectPath, out var a))
                {
                    IEnumerable<DependencyPackage> myPackages = a.Packages;
                    if (considerProjectReferences)
                        myPackages = myPackages.Concat(a.Projects);

                    packages = myPackages.ToAsyncEnumerable();
                }
                else
                {
                    packages = ReadProjectDependenciesAsync(projectPath, considerProjectReferences, cancellationToken);
                }

                packages = packages.Concat(this.FindNpmPackagesAsync(this.FileSystem.GetDirectoryName(projectPath), cancellationToken));
                projects.Add(
                    await ScannedProject.CreateAsync(
                        this.FileSystem.GetFileNameWithoutExtension(p),
                        packages
                    ).ConfigureAwait(false)
                );
            }

            return projects;
        }
        else
        {
            var packages = this.ReadProjectDependenciesAsync(this.SourcePath, considerProjectReferences, cancellationToken)
                .Concat(this.FindNpmPackagesAsync(this.FileSystem.GetDirectoryName(this.SourcePath), cancellationToken));

            return
            [
                await ScannedProject.CreateAsync(
                    this.FileSystem.GetFileNameWithoutExtension(this.SourcePath),
                    packages
                ).ConfigureAwait(false)
            ];
        }
    }

    private async IAsyncEnumerable<string> ReadFoldersAndProjectsFromSolutionAsync(string solutionPath, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var dictFolders = new Dictionary<Guid, string>();
        var dictProjects = new Dictionary<Guid, string>();
        var dictMappings = new Dictionary<Guid, Guid>();

        await foreach (var l in this.ReadLinesAsync(solutionPath, cancellationToken))
        {
            var match = SolutionFolderRegex().Match(l);
            if (match.Success)
            {
                // Solution Folder
                dictFolders[Guid.Parse(match.Groups[3].ValueSpan)] = match.Groups[1].Value;
            }
            else if ((match = SolutionProjectRegex().Match(l)).Success)
            {
                // Project
                dictProjects[Guid.Parse(match.Groups[2].ValueSpan)] = match.Groups[1].Value;
            }
            else if ((match = ProjectMappingRegex().Match(l)).Success)
            {
                // Mappings of projects to solution folders
                dictMappings[Guid.Parse(match.Groups[1].ValueSpan)] = Guid.Parse(match.Groups[2].ValueSpan);
            }
        }

        // Get GUIDs of Solutionfolders and Subfolders
        var includeFoldersGuids = new HashSet<Guid>();
        foreach (var kvp in dictFolders)
        {
            var folderGuid = kvp.Key;
            var list = new List<Guid>();

            // check solution folders recursively
            while (folderGuid != default)
            {
                list.Add(folderGuid);

                // check whether the name of the folder is included in the "include" list
                if (dictFolders.TryGetValue(folderGuid, out var folderName) && includeFolders.Contains(folderName))
                {
                    foreach (var guid in list)
                        includeFoldersGuids.Add(guid);
                    break;
                }

                // check whether solution folder is child of a parent folder
                if (dictMappings.TryGetValue(folderGuid, out folderGuid) == false)
                    break;
            }
        }

        foreach (var kvp in dictProjects)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (includeFolders.Count == 0 ||
                (dictMappings.TryGetValue(kvp.Key, out var folderGuid) // get folder Guid from project Guid
                && includeFoldersGuids.Contains(folderGuid))) // check if folder is in list of included folders
                yield return kvp.Value;
        }
    }


    private async Task<Dictionary<string, ProjectAssets>> FindAllAssetsAsync(string solutionPath, CancellationToken cancellationToken)
    {
        var projects = new Dictionary<string, ProjectAssets>(StringComparer.OrdinalIgnoreCase);

        await foreach (var f in this.FileSystem.FindFilesAsync(solutionPath, "project.assets.json", true, cancellationToken).ConfigureAwait(false))
        {
            if (Path.GetFileName(f.FullName) == "project.assets.json")
            {
                var assets = await ProjectAssets.ReadAsync(this.FileSystem, f, cancellationToken).ConfigureAwait(false);
                if (assets != null)
                {
                    if (!projects.TryGetValue(assets.ProjectPath, out var oldAssets) || oldAssets.SourceFile.LastModified < f.LastModified)
                        projects[assets.ProjectPath] = assets;
                }
            }
        }

        return projects;
    }
    private async IAsyncEnumerable<DependencyPackage> ReadProjectDependenciesAsync(string projectPath, bool considerProjectReferences, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        IAsyncEnumerable<DependencyPackage>? packages = null;
        var projectDir = this.FileSystem.GetDirectoryName(projectPath);
        var packagesConfigPath = this.FileSystem.Combine(projectDir, "packages.config");
        if (await this.FileSystem.FileExistsAsync(packagesConfigPath, cancellationToken).ConfigureAwait(false))
            packages = ReadPackagesConfigAsync(packagesConfigPath, cancellationToken);

        if (packages == null)
        {
            SimpleFileInfo? newest = null;

            await foreach (var f in this.FileSystem.FindFilesAsync(projectDir, "projects.assets.json", true, cancellationToken).ConfigureAwait(false))
            {
                if (Path.GetFileName(f.FullName) == "projects.assets.json" && (newest == null || newest.LastModified < f.LastModified))
                    newest = f;
            }

            var assetsPath = this.FileSystem.Combine(this.FileSystem.Combine(projectDir, "obj"), "project.assets.json");
            if (await this.FileSystem.FileExistsAsync(assetsPath, cancellationToken).ConfigureAwait(false))
                packages = ReadProjectAssetsAsync(assetsPath, considerProjectReferences, cancellationToken);
        }

        if (packages != null)
        {
            await foreach (var p in packages.ConfigureAwait(false))
                yield return p;
        }
    }

    private async IAsyncEnumerable<DependencyPackage> FindNpmPackagesAsync(string projectPath, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var npmScanner = new NpmDependencyScanner(this.CreateArgs with { SourcePath = projectPath });
        foreach (var npmProject in await npmScanner.ResolveDependenciesAsync(cancellationToken).ConfigureAwait(false))
        {
            foreach (var npmDependency in npmProject.Dependencies)
                yield return npmDependency;
        }
    }

    private async IAsyncEnumerable<DependencyPackage> ReadPackagesConfigAsync(string packagesConfigPath, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var stream = await this.FileSystem.OpenReadAsync(packagesConfigPath, cancellationToken).ConfigureAwait(false);
        var xdoc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken).ConfigureAwait(false);

        var packages = xdoc.Element("packages")?.Elements("package");
        if (packages == null)
            yield break;

        foreach (var p in packages)
        {
            var id = (string?)p.Attribute("id");
            if (string.IsNullOrWhiteSpace(id))
                continue;

            var version = (string?)p.Attribute("version");
            if (string.IsNullOrWhiteSpace(version))
                continue;

            yield return new DependencyPackage
            {
                Name = id,
                Version = version,
                Type = "nuget"
            };
        }
    }
    private async IAsyncEnumerable<DependencyPackage> ReadProjectAssetsAsync(string projectAssetsPath, bool considerProjectReferences, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var stream = await this.FileSystem.OpenReadAsync(projectAssetsPath, cancellationToken).ConfigureAwait(false);
        using var jdoc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        var libraries = jdoc.RootElement.GetProperty("libraries");
        if (libraries.ValueKind == JsonValueKind.Object)
        {
            foreach (var library in libraries.EnumerateObject())
            {
                if (library.Value.GetProperty("type").ValueEquals("package") || (library.Value.GetProperty("type").ValueEquals("project") && considerProjectReferences))
                {
                    var parts = library.Name.Split('/', 2);

                    yield return new DependencyPackage
                    {
                        Name = parts[0],
                        Version = parts[1],
                        Type = "nuget"
                    };
                }
            }
        }
    }

    private sealed class ProjectAssets
    {
        private ProjectAssets(SimpleFileInfo sourceFile, string projectName, string projectPath, IReadOnlyCollection<DependencyPackage> packages, IReadOnlyCollection<DependencyPackage> projects)
        {
            this.SourceFile = sourceFile;
            this.ProjectName = projectName;
            this.ProjectPath = projectPath;
            this.Packages = packages;
            this.Projects = projects;
        }

        public SimpleFileInfo SourceFile { get; }
        public string ProjectName { get; }
        public string ProjectPath { get; }
        public IReadOnlyCollection<DependencyPackage> Packages { get; }
        public IReadOnlyCollection<DependencyPackage> Projects { get; }

        public static async Task<ProjectAssets?> ReadAsync(ISourceFileSystem fileSystem, SimpleFileInfo file, CancellationToken cancellationToken)
        {
            using var stream = await fileSystem.OpenReadAsync(file.FullName, cancellationToken).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            if (!doc.RootElement.TryGetProperty("project", out var project) || project.ValueKind != JsonValueKind.Object)
                return null;

            if (!project.TryGetProperty("restore", out var restore) || restore.ValueKind != JsonValueKind.Object)
                return null;

            if (!restore.TryGetProperty("projectName", out var name) || name.ValueKind != JsonValueKind.String)
                return null;

            if (!restore.TryGetProperty("projectPath", out var path) || path.ValueKind != JsonValueKind.String)
                return null;

            var packages = new List<DependencyPackage>();
            var projectPackages = new List<DependencyPackage>();

            if (doc.RootElement.TryGetProperty("libraries", out var libraries) && libraries.ValueKind == JsonValueKind.Object)
            {
                foreach (var library in libraries.EnumerateObject())
                {
                    var type = library.Value.GetProperty("type");
                    bool isPackage = type.ValueEquals("package");
                    if (isPackage || type.ValueEquals("project"))
                    {
                        var parts = library.Name.Split('/', 2);
                        (isPackage ? packages : projectPackages).Add(
                            new DependencyPackage
                            {
                                Name = parts[0],
                                Version = parts[1],
                                Type = "nuget"
                            }
                        );
                    }
                }
            }

            return new ProjectAssets(file, name.GetString()!, path.GetString()!, packages, projectPackages);
        }
    }

    [GeneratedRegex(@"^Project[^=]*=\s*""[^""]+""\s*,\s*""(?<1>[^""]+)"",\s""(?<2>[^""]+)""", RegexOptions.ExplicitCapture | RegexOptions.Singleline)]
    private static partial Regex SolutionProjectRegex();
    [GeneratedRegex(@"^Project\(""{2150[Ee]333-8[Ff][Dd][Cc]-42[Aa]3-9474-1[Aa]3956[Dd]46[Dd][Ee]8}""\)\s=\s""(?<1>[^""]+)"",\s""(?<2>[^""]+)"",\s""(?<3>[^""]+)""", RegexOptions.ExplicitCapture | RegexOptions.Singleline)]
    private static partial Regex SolutionFolderRegex();
    [GeneratedRegex(@"{(?<1>[^}]+)}\s=\s{(?<2>[^}]+)}", RegexOptions.ExplicitCapture | RegexOptions.Singleline)]
    private static partial Regex ProjectMappingRegex();
}
