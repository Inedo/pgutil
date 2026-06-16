using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

partial class Program
{
    private sealed partial class ContainersCommand
    {
        private sealed class AuditCommand : IConsoleCommand
        {
            public static string Name => "audit";
            public static string Description => "List packages used by a container and show known vulnerabilities";
            public static string Examples => """
                  $> pgutil containers audit --feed=internal-docker --image=nginx:1.83
                  $> pgutil containers audit --feed=local-docker --image=sha256:072845d6e9a282e00aa465448698bf134c85655ebf2004527dcba19c657ccd47
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<ImageOption>()
                    .WithOption<ArchOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feed = context.GetFeedName();
                var tagOrDigest = context.GetOption<ImageOption>();

                CM.WriteLine("Requesting audit of packages in ", new TextSpan(tagOrDigest, ConsoleColor.White), "...");

                var results = await client.AuditContainerImageAsync(feed, tagOrDigest, context.GetOptionOrDefault<ArchOption>(), cancellationToken);

                CM.WriteLine($"Repository: {results.Repository}");
                CM.Write("Tags: ");
                if (results.Tags?.Length > 0)
                    CM.WriteLine(string.Join(", ", results.Tags));
                else
                    CM.WriteLine(ConsoleColor.DarkGray, "none");

                CM.WriteLine($"Digest: {results.Digest}");
                CM.WriteLine($"Created: {results.Created.ToLocalTime()}");

                if (results.ManifestList?.Length > 0)
                {
                    CM.WriteLine("Subimages:");
                    foreach (var m in results.ManifestList)
                    {
                        CM.Write("  ");
                        if (!string.IsNullOrEmpty(m.Architecture))
                            CM.Write(new TextSpan(m.Architecture, ConsoleColor.Blue), ": ");
                        CM.WriteLine(m.Digest);
                    }
                }

                bool hasContain = false;

                if (results.Packages?.Length > 0)
                {
                    CM.WriteLine($"Total Packages: {results.Packages?.Length ?? 0}");
                    CM.WriteLine();

                    if (results.Packages?.Length > 0)
                    {
                        var vulnLookup = new Dictionary<int, List<VulnerabilityInfo>>();
                        if (results.Vulnerabilities is not null)
                        {
                            foreach (var vuln in results.Vulnerabilities)
                            {
                                if (vuln.AffectedPackages is not null)
                                {
                                    foreach (int index in vuln.AffectedPackages)
                                    {
                                        if (!vulnLookup.TryGetValue(index, out var list))
                                        {
                                            list = [];
                                            vulnLookup.Add(index, list);
                                        }

                                        list.Add(vuln);
                                    }
                                }
                            }
                        }

                        CM.WriteLine("-= Packages =-");

                        for (int i = 0; i < results.Packages.Length; i++)
                        {
                            CM.WriteLine();
                            var package = PUrl.Parse(results.Packages[i]);
                            CM.WriteLine(ConsoleColor.Blue, $"{package.GroupAndName} {package.PackageVersion}");
                            CM.Write("Vulnerabilities: ");
                            if (vulnLookup.TryGetValue(i, out var vulns))
                            {
                                foreach (var vuln in vulns)
                                {
                                    hasContain |= string.Equals(vuln.Assessment, "Contain", StringComparison.OrdinalIgnoreCase);

                                    CM.Write(new TextSpan(vuln.Id, ConsoleColor.White), " ");

                                    if (vuln.Pvrs.HasValue)
                                        CM.Write(vuln.AssessmentColor ?? vuln.CategoryColor, $"Category {vuln.Pvrs.GetValueOrDefault()} ({vuln.Assessment})");
                                    else if (vuln.NumericCvss.HasValue)
                                        CM.Write(new TextSpan($"{vuln.NumericCvss.GetValueOrDefault():F1} ({vuln.Severity})", vuln.SeverityColor));

                                    CM.WriteLine();
                                    CM.WriteLine($"  {vuln.Summary}");
                                }
                            }
                            else
                            {
                                CM.WriteLine(ConsoleColor.Green, "none");
                            }
                        }
                    }
                }

                if (hasContain)
                {
                    CM.WriteLine();
                    CM.WriteError("Image contains vulnerabilies assessed as Contain.");
                    return -1;
                }

                return 0;
            }

            private sealed class ImageOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--image";
                public static string Description => "Image to audit. May be in repository:tag format or a specfic digest";
            }

            private sealed class ArchOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--arch";
                public static string Description => "Architecture of subimage. This is only used if the image specified is a manifest list";
            }
        }
    }
}