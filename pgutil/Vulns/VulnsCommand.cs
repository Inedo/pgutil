using ConsoleMan;
using Inedo.DependencyScan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed class VulnsCommand : IConsoleCommandContainer
{
    public static string Name => "vulns";
    public static string Description => "Audit packages and assess vulnerabilities";

    public static void Configure(ICommandBuilder builder)
    {
        builder.WithCommand<AuditCommand>()
            .WithCommand<AssessCommand>();
    }

    internal sealed class AuditCommand : IConsoleCommand
    {
        public static string Name => "audit";
        public static string Description => "List vulnerabilities associated with a package or project file";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<PackageOption>()
                .WithOption<PackageTypeOption>()
                .WithOption<VersionOption>()
                .WithOption<InputOption>();
        }

        public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var client = context.GetProGetClient();

            if (context.TryGetOption<PackageOption>(out var package))
            {
                if (!context.TryGetOption<VersionOption>(out var version))
                {
                    CM.WriteError<VersionOption>("Version is required when --package is specified");
                    return -1;
                }

                if (!context.TryGetOption<PackageTypeOption>(out var type))
                {
                    CM.WriteError<PackageTypeOption>("Type is required when --package is specified");
                    return -1;
                }

                string? group = null;
                var name = package;
                int index = package.LastIndexOf('/');
                if (index >= 0)
                {
                    group = package[..index];
                    name = package[(index + 1)..];
                }

                return await AuditAsync(client, [new DependencyInfo(new PackageVersionIdentifier(type, name, version, group), [])], cancellationToken);
            }
            else if (context.TryGetOption<InputOption>(out var input))
            {
                var scannerType = DependencyScannerType.Auto;
                if (context.TryGetOption<PackageTypeOption>(out var type))
                {
                    if (!Enum.TryParse(type, true, out scannerType))
                    {
                        CM.WriteError<PackageTypeOption>("Invalid type specified.");
                        return -1;
                    }
                }

                CM.WriteLine("Scanning for dependencies in ", new TextSpan(input, ConsoleColor.White), "...");

                var scanner = DependencyScanner.GetScanner(new CreateDependencyScannerArgs(input, SourceFileSystem.Default), scannerType);
                var projects = await scanner.ResolveDependenciesAsync(cancellationToken);
                if (projects.Count == 0)
                {
                    CM.WriteError($"No projects found in input path: {input}");
                    return -1;
                }

                var dependencyLookup = new Dictionary<PackageVersionIdentifier, List<string>>();
                foreach (var p in projects)
                {
                    foreach (var d in p.Dependencies)
                    {
                        var pvi = new PackageVersionIdentifier(d.Type!, d.Name, d.Version, d.Group);

                        if (!dependencyLookup.TryGetValue(pvi, out var projs))
                        {
                            projs = [];
                            dependencyLookup[pvi] = projs;
                        }

                        projs.Add(p.Name);
                    }
                }

                var infos = dependencyLookup.Select(d => new DependencyInfo(d.Key, d.Value)).ToList();

                CM.WriteLine($"Found {infos.Count} package dependencies. Performing audit...");

                return await AuditAsync(client, infos, cancellationToken);
            }
            else
            {
                CM.WriteError("Must specify --package, --version, --type or --input.");
                return -1;
            }
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
                if (vuln.NumericCvss.HasValue)
                    CM.Write(new TextSpan($"{vuln.NumericCvss.GetValueOrDefault():F1} ({vuln.Severity})", getSeverityColor(vuln.NumericCvss.GetValueOrDefault())), " - ");

                WordWrapper.WriteOutput(vuln.Summary, vuln.Id.Length + 2);
                Console.WriteLine();
                CM.Write(" * Packages: ");
                WordWrapper.WriteOutput(string.Join(", ", vuln.AffectedPackages.Select(i => infos[i].ToPackageString())), 13);
                Console.WriteLine();
                CM.Write(" * Projects: ");
                WordWrapper.WriteOutput(string.Join(", ", vuln.AffectedPackages.SelectMany(i => infos[i].Projects).Distinct()), 13);
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

        private sealed class PackageTypeOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--type";
            public static string Description => "Type of package to audit for vulnerabilities";
            public static string[] ValidValues => ["apk", "deb", "maven", "nuget", "conda", "cran", "helm", "npm", "pypi", "rpm", "gem"];
        }

        private sealed class PackageOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--package";
            public static string Description => "Name of package to audit for vulnerabilities";
        }

        private sealed class VersionOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--version";
            public static string Description => "Version of package to audit for vulnerabilities";
        }

        private sealed class InputOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--input";
            public static string Description => "Project to audit for vulnerable packages";
        }
    }

    internal sealed class AssessCommand : IConsoleCommand
    {
        public static string Name => "assess";
        public static string Description => "Assess a vulneability by specifying its ID";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<VulnIdOption>()
                .WithOption<AssessmentTypeOption>()
                .WithOption<CommentOption>()
                .WithOption<PolicyOption>();
        }

        public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var client = context.GetProGetClient();

            var id = context.GetOption<VulnIdOption>();
            var type = context.GetOption<AssessmentTypeOption>();
            _ = context.TryGetOption<CommentOption>(out var comment);
            _ = context.TryGetOption<PolicyOption>(out var policy);

            CM.Write("Assessing ", new TextSpan(id, ConsoleColor.White), " as ", new TextSpan(type, ConsoleColor.White));

            if (!string.IsNullOrWhiteSpace(policy))
                CM.Write(" with policy ", new TextSpan(policy, ConsoleColor.White));

            if (!string.IsNullOrWhiteSpace(comment))
            {
                Console.Write(" (comment: ");
                WordWrapper.WriteOutput(comment);
                Console.Write(')');
            }

            Console.WriteLine("...");

            await client.AssessVulnerabilityAsync(id, type, comment, policy, cancellationToken);

            Console.WriteLine("Assessment completed.");

            return 0;
        }

        private sealed class VulnIdOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--id";
            public static string Description => "ID of the vulnerability to assess";
        }

        private sealed class AssessmentTypeOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--type";
            public static string Description => "Assessment type in ProGet";
        }

        private sealed class CommentOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--comment";
            public static string Description => "Comment to add to the vulnerability in ProGet";
        }

        private sealed class PolicyOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--policy";
            public static string Description => "ProGet policy to apply to the assessment";
        }
    }
}
