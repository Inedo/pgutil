using System.Runtime.CompilerServices;
using System.Text;
using Inedo.ProGet.Scan;

namespace Inedo.DependencyScan;

/// <summary>
/// Used to generate a list of dependencies consumed by projects.
/// </summary>
public abstract class DependencyScanner
{
    private protected DependencyScanner(CreateDependencyScannerArgs args)
    {
        this.CreateArgs = args;
    }

    /// <summary>
    /// Gets the source path to scan.
    /// </summary>
    public string SourcePath => this.CreateArgs.SourcePath;
    /// <summary>
    /// Gets the source file system abstraction used by the scanner.
    /// </summary>
    public ISourceFileSystem FileSystem => this.CreateArgs.FileSystem;
    /// <summary>
    /// Gets the type of scanner.
    /// </summary>
    public abstract DependencyScannerType Type { get; }

    private protected CreateDependencyScannerArgs CreateArgs { get; }

    /// <summary>
    /// Returns the dependencies used by each project in the specified <see cref="SourcePath"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for asynchronous operation.</param>
    /// <returns>Dependencies used by each project.</returns>
    public abstract Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a <see cref="DependencyScanner"/> for the specified path.
    /// </summary>
    /// <param name="args">Options for the dependency scanner.</param>
    /// <param name="type">Type of project to scan for.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see cref="DependencyScanner"/> for the specified path.</returns>
    public static async Task<DependencyScanner> GetScannerAsync(CreateDependencyScannerArgs args, DependencyScannerType type = DependencyScannerType.Auto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.FileSystem);

        args = args with { SourcePath = Path.Combine(Environment.CurrentDirectory, args.SourcePath) };

        if (type == DependencyScannerType.Auto)
        {
            var (scannerType, filePath) = await GetImplicitTypeAsync(args.FileSystem, args.SourcePath, cancellationToken).ConfigureAwait(false);
            type = scannerType;
            args = args with { SourcePath = filePath };
            if (type == DependencyScannerType.Auto)
                throw new DependencyScannerException("Could not automatically determine project type.");
        }
        else if (string.IsNullOrWhiteSpace(args.SourcePath))
        {
            var file = await GetImplicitFileAsync(type, args.SourcePath, cancellationToken).ConfigureAwait(false);
            args = args with { SourcePath = file };
        }

        return type switch
        {
            DependencyScannerType.NuGet => new NuGetDependencyScanner(args),
            DependencyScannerType.Npm => new NpmDependencyScanner(args),
            DependencyScannerType.PyPI => new PypiDependencyScanner(args),
            DependencyScannerType.Conda => new CondaDependencyScanner(args),
            DependencyScannerType.Cargo => new CargoDependencyScanner(args),
            DependencyScannerType.Composer => new ComposerDependencyScanner(args),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private protected async IAsyncEnumerable<string> ReadLinesAsync(string path, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(await this.FileSystem.OpenReadAsync(path, cancellationToken).ConfigureAwait(false), Encoding.UTF8);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false)) != null)
        {
            yield return line;
        }
    }

    private static async Task<string> GetImplicitFileAsync(DependencyScannerType scannerType, string folder, CancellationToken cancellationToken = default)
    {
        if (scannerType == DependencyScannerType.Npm)
            return folder;
        else if (scannerType == DependencyScannerType.Cargo)
            return folder;
        else if (scannerType == DependencyScannerType.Composer)
            return folder;
        else if (scannerType == DependencyScannerType.NuGet)
        {
            var files = await SourceFileSystem.Default.FindFilesAsync(folder, "*.sln", true, cancellationToken).ToListAsync(cancellationToken);
            if (files.Count == 1)
                return files[0].FullName;
            if (files.Count > 1)
                throw new DependencyScannerException("Multiple solution files found in directory.  Specify which solution file you would like to scan be using \"--input\" argument.");
            files = await SourceFileSystem.Default.FindFilesAsync(folder, "*.csproj", true, cancellationToken).ToListAsync(cancellationToken);
            if (files.Count == 1)
                return files[0].FullName;
            if (files.Count > 1)
                throw new DependencyScannerException("Multiple project files found in directory.  Specify which project file you would like to scan be using \"--input\" argument.");
            throw new DependencyScannerException("No solution or project files found in directory. Specify which file you would like to scan be using \"--input\" argument.");
        }
        else if (scannerType == DependencyScannerType.PyPI || scannerType == DependencyScannerType.Conda)
        {
            var files = await SourceFileSystem.Default.FindFilesAsync(folder, "requirements.txt", true, cancellationToken).ToListAsync(cancellationToken);
            if (files.Count == 1)
                return files[0].FullName; 
            if (files.Count > 1)
                throw new DependencyScannerException("Multiple requirements.txt files found in directory.  Specify which requirements.txt file you would like to scan be using \"--input\" argument.");

            throw new DependencyScannerException("No requirements.txt files found in directory. Specify which file you would like to scan be using \"--input\" argument.");
        }
        throw new DependencyScannerException("Could not automatically determine project type.");
    }

    private static async Task<(DependencyScannerType scannerType, string filePath)> GetImplicitTypeAsync(ISourceFileSystem fileSystem, string fileName, CancellationToken cancellationToken = default)
    {
        if(File.GetAttributes(fileName).HasFlag(FileAttributes.Directory))
        {
            var files = await fileSystem.FindFilesAsync(fileName, "*.sln", true, cancellationToken).ToListAsync(cancellationToken);
            if(files.Count == 1)
                return (scannerType: DependencyScannerType.NuGet, filePath: files[0].FullName);
            else if (files.Count > 1)
                throw new DependencyScannerException("Multiple solution files found in directory.  Specify which solution file you would like to scan be using \"--input\" argument.");

            files = await fileSystem.FindFilesAsync(fileName, "*.csproj", true, cancellationToken).ToListAsync(cancellationToken);
            if(files.Count == 1)
                return (scannerType: DependencyScannerType.NuGet, filePath: files[0].FullName);
            else if (files.Count > 1)
                throw new DependencyScannerException("Multiple project files found in directory.  Specify which project file you would like to scan be using \"--input\" argument.");

            files = await fileSystem.FindFilesAsync(fileName, "package-lock.json", true, cancellationToken).ToListAsync(cancellationToken);
            if(files.Count > 0)
                return (scannerType: DependencyScannerType.Npm, filePath: fileName);
            
            files = await fileSystem.FindFilesAsync(fileName, "Cargo.lock", true, cancellationToken).ToListAsync(cancellationToken);
            if(files.Count > 0)
                return (scannerType: DependencyScannerType.Cargo, filePath: fileName);
            
            files = await fileSystem.FindFilesAsync(fileName, "composer.lock", true, cancellationToken).ToListAsync(cancellationToken);
            if(files.Count > 0)
                return (scannerType: DependencyScannerType.Composer, filePath: fileName);

            files = await fileSystem.FindFilesAsync(fileName, "requirements.txt", true, cancellationToken).ToListAsync(cancellationToken);
            if (files.Count == 1)
            {
                var type = getPythonScannerType(files[0].FullName);
                return (scannerType: type, filePath: files[0].FullName);
            }
            else if (files.Count > 1)
                throw new DependencyScannerException("Multiple requirements.txt files found in directory.  Specify which requirements.text file you would like to scan be using \"--input\" argument.");

            return (DependencyScannerType.Auto, fileName);
        }

        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".sln" or ".csproj" => (DependencyScannerType.NuGet, fileName),
            ".toml" => (DependencyScannerType.Cargo, fileName),
            ".lock" => Path.GetFileName(fileName).Equals("Cargo.lock", StringComparison.OrdinalIgnoreCase) ? (DependencyScannerType.Cargo, fileName) : (DependencyScannerType.Composer, fileName),
            ".json" => Path.GetFileName(fileName).Equals("composer.json", StringComparison.OrdinalIgnoreCase) ? (DependencyScannerType.Composer, fileName) : (DependencyScannerType.Npm, fileName),
            _ => Path.GetFileName(fileName).Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ? (getPythonScannerType(fileName), fileName) : (DependencyScannerType.Auto, fileName)
        };
        static DependencyScannerType getPythonScannerType(string fileName)
        {
            try
            {
                using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fileStream, Encoding.UTF8);
                for (int i = 0; i < 4; i++)
                {
                    var line = reader.ReadLine();
                    if (line != null && ((line.StartsWith('#') && line.Contains("conda create", StringComparison.OrdinalIgnoreCase)) || line.StartsWith("@EXPLICIT", StringComparison.OrdinalIgnoreCase)))
                        return DependencyScannerType.Conda;
                }
            }
            catch { }

            return DependencyScannerType.PyPI;
        }
    }
}
