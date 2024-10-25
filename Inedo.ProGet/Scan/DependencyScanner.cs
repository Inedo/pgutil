using System.Runtime.CompilerServices;
using System.Text;

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
    /// <returns><see cref="DependencyScanner"/> for the specified path.</returns>
    public static DependencyScanner GetScanner(CreateDependencyScannerArgs args, DependencyScannerType type = DependencyScannerType.Auto)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(args.FileSystem);
        ArgumentNullException.ThrowIfNull(args.SourcePath);

        args = args with { SourcePath = Path.Combine(Environment.CurrentDirectory, args.SourcePath) };

        if (type == DependencyScannerType.Auto)
        {
            type = GetImplicitType(args.SourcePath);
            if (type == DependencyScannerType.Auto)
                throw new DependencyScannerException("Could not automatically determine project type.");
        }

        return type switch
        {
            DependencyScannerType.NuGet => new NuGetDependencyScanner(args),
            DependencyScannerType.Npm => new NpmDependencyScanner(args),
            DependencyScannerType.PyPI => new PypiDependencyScanner(args),
            DependencyScannerType.Conda => new CondaDependencyScanner(args),
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

    private static DependencyScannerType GetImplicitType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".sln" or ".csproj" => DependencyScannerType.NuGet,
            ".json" => DependencyScannerType.Npm,
            _ => Path.GetFileName(fileName).Equals("requirements.txt", StringComparison.OrdinalIgnoreCase) ? getPythonScannerType(fileName) : DependencyScannerType.Auto
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
