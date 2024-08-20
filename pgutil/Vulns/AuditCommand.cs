using ConsoleMan;
using Inedo.DependencyScan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class VulnsCommand
{
    internal sealed class AuditCommand : IConsoleCommand
    {
        public static string Name => "audit";
        public static string Description => "Lists vulnerabilities associated with a project file";
        public static string Examples => """
              >$ pgutil vulns audit --project=c:\application-files\application.csproj

              >$ pgutil vulns audit --project=c:\projects\abc-nuget-project.csproj --type=nuget

            For more information, see: https://docs.inedo.com/docs/proget/reference-api/vulnerabilities/proget-api-vulnerabilties-audit
            """;

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithOption<PackageTypeOption>()
                .WithOption<ProjectOption>();
        }

        public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var client = context.GetProGetClient();

            var input = context.GetOption<ProjectOption>();

            if (!File.Exists(input) && !Directory.Exists(input))
            {
                CM.WriteError<ProjectOption>($"Project not found at {Path.GetFullPath(input)}");
                return -1;
            }

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

        private sealed class PackageTypeOption : IConsoleOption
        {
            public static bool Required => false;
            public static string Name => "--type";
            public static string Description => "Type of package to audit for vulnerabilities";
            public static string[] ValidValues => ["apk", "deb", "maven", "nuget", "conda", "cran", "helm", "npm", "pypi", "rpm", "gem"];
        }

        private sealed class ProjectOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--project";
            public static string Description => "Project to audit for vulnerable packages";
        }
    }
}
