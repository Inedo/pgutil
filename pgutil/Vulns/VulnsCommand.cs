using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class VulnsCommand : IConsoleCommandContainer
{
    public static string Name => "vulns";
    public static string Description => "Audits packages and assesses vulnerabilities";

    public static void Configure(ICommandBuilder builder)
    {
        builder.WithProGetClientOptions()
            .WithCommand<AuditCommand>()
            .WithCommand<AssessCommand>();
    }

    private static async Task<int> AuditAsync(ProGetClient client, List<DependencyInfo> infos, CancellationToken cancellationToken)
    {
        var vulns = await client.AuditPackagesForVulnerabilitiesAsync(infos.Select(p => p.Package).ToList(), cancellationToken).ToListAsync();
        // show most critical vulns first
        vulns.Sort((v1, v2) => -v1.NumericCvss.GetValueOrDefault().CompareTo(v2.NumericCvss.GetValueOrDefault()));

        switch (vulns.Count)
        {
            case 0:
                CM.WriteLine(ConsoleColor.Green, "No vulnerable packages detected!");
                break;

            case 1:
                CM.WriteLine(ConsoleColor.Yellow, "1 vulnerable package detected.");
                Console.WriteLine();
                break;

            default:
                CM.WriteLine(ConsoleColor.Yellow, $"{vulns.Count} vulnerable packages detected.");
                Console.WriteLine();
                break;
        }

        foreach (var vuln in vulns)
        {
            CM.Write(new TextSpan(vuln.Id, ConsoleColor.White), ": ");
            if (vuln.Pvrs.HasValue)
            {
                CM.Write(new TextSpan($"Category {vuln.Pvrs.GetValueOrDefault()} ({vuln.Assessment})", getAssessmentColor(vuln.Assessment) ?? getCategoryColor(vuln.Pvrs.GetValueOrDefault())), " - ");
                Console.WriteLine();
                CM.WriteLine($"CVSS Score: {vuln.Cvss} ({vuln.NumericCvss.GetValueOrDefault():F1})");
            }
            else if (vuln.NumericCvss.HasValue)
                CM.Write(new TextSpan($"{vuln.NumericCvss.GetValueOrDefault():F1} ({vuln.Severity})", getSeverityColor(vuln.NumericCvss.GetValueOrDefault())), " - ");

            var affectedPackages = vuln.AffectedPackages ?? [];
            WordWrapper.WriteOutput(vuln.Summary, vuln.Id.Length + 2);
            Console.WriteLine();
            CM.Write(" * Packages: ");
            WordWrapper.WriteOutput(string.Join(", ", affectedPackages.Select(i => infos[i].ToPackageString())), 13);
            Console.WriteLine();
            CM.Write(" * Projects: ");
            WordWrapper.WriteOutput(string.Join(", ", affectedPackages.SelectMany(i => infos[i].Projects).Distinct()), 13);
            Console.WriteLine();
            Console.WriteLine();
        }

        return vulns.Count;

        static ConsoleColor getSeverityColor(decimal score)
        {
            return score switch
            {
                >= 7 => ConsoleColor.Red,
                >= 4 => ConsoleColor.DarkYellow,
                _ => ConsoleColor.Yellow
            };
        }

        static ConsoleColor? getAssessmentColor(string? severity)
        {
            return severity switch
            {
                "Contain" => ConsoleColor.Red,
                "Remediate" => ConsoleColor.DarkYellow,
                "Monitor" => ConsoleColor.Green,
                _ => null
            };
        }

        static ConsoleColor getCategoryColor(int category)
        {
            return category switch
            {
                5 => ConsoleColor.Red,
                4 => ConsoleColor.Red,
                3 => ConsoleColor.DarkYellow,
                2 => ConsoleColor.Green,
                //1 is actually FAFAFA,but that color blends with the background
                1 => ConsoleColor.Green,
                _ => ConsoleColor.Green
            };
        }
    }

    private sealed record class DependencyInfo(PackageVersionIdentifier Package, List<string> Projects)
    {
        public string ToPackageString()
        {
            var name = this.Package.Name;
            if (!string.IsNullOrEmpty(this.Package.Group))
                name = $"{this.Package.Group}/{this.Package.Name}";

            return $"{name} {this.Package.Version}";
        }
    }
}
