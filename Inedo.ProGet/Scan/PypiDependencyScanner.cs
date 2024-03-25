using System.Runtime.CompilerServices;

namespace Inedo.DependencyScan;

internal sealed class PypiDependencyScanner(CreateDependencyScannerArgs args) : DependencyScanner(args)
{
    public override DependencyScannerType Type => DependencyScannerType.PyPI;

    public override async Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default)
    {
        return [new ScannedProject("PyPiPackage", await this.ReadDependenciesAsync(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false))];
    }

    private async IAsyncEnumerable<DependencyPackage> ReadDependenciesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var line in this.ReadLinesAsync(this.SourcePath, cancellationToken).ConfigureAwait(false))
        {
            var parts = line.Split("==", 2, StringSplitOptions.None);
            if (parts.Length == 2)
                yield return new DependencyPackage { Name = parts[0], Version = parts[1] };
        }
    }
}
