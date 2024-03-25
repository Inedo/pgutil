using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Inedo.DependencyScan;

internal sealed partial class CondaDependencyScanner(CreateDependencyScannerArgs args) : DependencyScanner(args)
{
    public override DependencyScannerType Type => DependencyScannerType.Conda;

    public override async Task<IReadOnlyCollection<ScannedProject>> ResolveDependenciesAsync(CancellationToken cancellationToken = default)
    {
        return [new ScannedProject("CondaPackage", await this.ReadDependenciesAsync(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false))];
    }

    private async IAsyncEnumerable<DependencyPackage> ReadDependenciesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        bool parseExplicit = false;
        string? platform = null; //subdir

        await foreach (var line in this.ReadLinesAsync(this.SourcePath, cancellationToken).ConfigureAwait(false))
        {
            if (line.StartsWith('#'))
            {
                var platformIndex = line.IndexOf("platform:", StringComparison.OrdinalIgnoreCase);
                if (platformIndex > 0)
                    platform = line[(platformIndex + 9)..].Trim();
                continue;
            }

            if (line.StartsWith("@EXPLICIT", StringComparison.OrdinalIgnoreCase))
            {
                parseExplicit = true;
                continue;
            }

            if (parseExplicit)
            {
                var match = UrlRegex().Match(line.Trim());
                if (match.Success)
                {
                    var subdir = Uri.UnescapeDataString(match.Groups[1].Value);
                    var fullName = Uri.UnescapeDataString(match.Groups[2].Value);
                    int buildIndex = fullName.LastIndexOf('-');
                    var build = fullName[(buildIndex + 1)..];
                    int versionIndex = fullName.LastIndexOf('-', buildIndex - 1);
                    var version = fullName.Substring(versionIndex + 1, buildIndex - versionIndex - 1);
                    var name = fullName[..versionIndex];

                    var qualifier = new List<string> { $"build={Uri.EscapeDataString(build)}" };
                    if (platform != null)
                        qualifier.Add($"subdir={Uri.EscapeDataString(subdir)}");

                    qualifier.Add($"type={match.Groups[3].ValueSpan}");

                    yield return new DependencyPackage { Name = name, Version = version, Qualifier = string.Join('&', qualifier) };
                }
            }
            else
            {
                var parts = line.Split('=', 3, StringSplitOptions.None);
                if (parts.Length == 3)
                {
                    var qualifier = new List<string> { $"build={Uri.EscapeDataString(parts[2])}" };
                    if (platform != null)
                        qualifier.Add($"subdir={Uri.EscapeDataString(platform)}");

                    yield return new DependencyPackage { Name = parts[0], Version = parts[1], Qualifier = string.Join('&', qualifier) };
                }
            }
        }
    }

    [GeneratedRegex(@"^https?://.+/(?<1>[^/]+)/(?<2>[^/]+)\.(?<3>tar\.bz2|conda)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    private static partial Regex UrlRegex();
}
