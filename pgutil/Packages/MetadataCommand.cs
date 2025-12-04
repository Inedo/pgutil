using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class MetadataCommand : IConsoleCommand
        {
            public static string Name => "metadata";
            public static string Description => "Displays metadata about a package";
            public static string Examples => """
                 > pgutil packages packages metadata --package="MyPackage" --version=1.2.3   
                 > pgutil packages packages metadata --package="Newtonsoft.Json" --version=12.0.3 --feed=public-nuget  

                For more information, see: https://docs.inedo.com/docs/proget/api/packages/metadata 
                """;
            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>()
                    .WithOption<PackageVersionOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var (p, _) = GetPackageIdentifier(context);

                var metadata = await client.GetPackageMetadataAsync(p, cancellationToken);

                CM.WriteLine(ConsoleColor.White, $"{metadata.Name} {metadata.Version}");
                CM.WriteLine($" Published: {metadata.Published?.ToLocalTime()} by {metadata.PublishedBy}");
                if (metadata.Size.HasValue)
                    CM.WriteLine($" Size: {FormatSize(metadata.Size.GetValueOrDefault())}");
                if (metadata.Downloads.HasValue)
                    CM.WriteLine($" Downloads: {metadata.Downloads:N0}");
                CM.WriteLine($" Status: {metadata.State}{GetStatusText(metadata.Status)}");

                if (metadata.Vulnerabilities?.Length > 0)
                {
                    CM.WriteLine(" Vulnerabilities:");
                    foreach (var vuln in metadata.Vulnerabilities)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        CM.Write($"  {vuln.Id}: ");
                        WordWrapper.WriteOutput(vuln.Summary, 3);
                        Console.ForegroundColor = ConsoleColor.Gray;

                        CM.WriteLine();
                    }
                }

                if (metadata.Artifacts?.Length > 0)
                {
                    CM.WriteLine($" Artifacts ({metadata.Artifacts.Length}):");
                    foreach (var a in metadata.Artifacts)
                    {
                        CM.WriteLine($"  {a.Name ?? a.Qualifier}");
                        if (a.Size.HasValue)
                            CM.WriteLine($"   Size: {FormatSize(a.Size.GetValueOrDefault())}");
                        if (a.Downloads.HasValue)
                            CM.WriteLine($"   Downloads: {a.Downloads:N0}");
                        CM.WriteLine($" Status: {metadata.State}{GetStatusText(metadata.Status)}");

                        CM.WriteLine();
                    }
                }

                if (metadata.ComplianceReport is not null)
                {
                    CM.WriteLine(" Compliance: ", new TextSpan(metadata.ComplianceReport.AnalysisResult, GetComplianceColor(metadata.ComplianceReport.AnalysisResult)));
                    if (metadata.ComplianceReport.Issues?.Length > 0)
                    {
                        foreach (var i in metadata.ComplianceReport.Issues)
                            CM.WriteLine(ConsoleColor.Yellow, $"  {i}");
                    }
                }

                CM.Write(" License: ");
                if (metadata.Licenses?.Length > 0)
                    CM.WriteLine(string.Join(", ", metadata.Licenses.Select(l => l.Code)));
                else
                    CM.WriteLine(ConsoleColor.Yellow, "none/unknown");

                CM.WriteLine();
                return 0;
            }

            private static string FormatSize(long size)
            {
                return size switch
                {
                    < 10 * 1024 => size.ToString("N0"),
                    < 10 * 1024 * 1024 => $"{size / 1024:N0} KB",
                    < 1024 * 1024 * 1024 => $"{size / (1024 * 1024):N0} MB",
                    _ => $"{size / (1024 * 1024 * 1024):N0} GB"
                };
            }

            private static ConsoleColor GetComplianceColor(string result)
            {
                return result switch
                {
                    "Compliant" => ConsoleColor.Green,
                    "Warn" => ConsoleColor.Yellow,
                    "Inconclusive" => ConsoleColor.Blue,
                    _ => ConsoleColor.Red
                };
            }

            private static string GetStatusText(PackageMetadataStatus? status)
            {
                if (status is null)
                    return string.Empty;

                var items = new List<string>();
                if (status.Deprecated)
                    items.Add("Deprecated");
                if (!status.Listed)
                    items.Add("Unlisted");
                if (!status.AllowDownload.GetValueOrDefault())
                    items.Add("Download Blocked");

                if (items.Count == 0)
                    return string.Empty;

                return string.Join(", ", items);
            }
        }
    }
}
