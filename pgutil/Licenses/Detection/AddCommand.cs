using System.Text.RegularExpressions;
using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class DetectionCommand
        {
            private sealed partial class AddCommand : IConsoleCommand
            {
                public static string Name => "add";
                public static string Description => "Adds a detection type to a license";

                public static void Configure(ICommandBuilder builder)
                {
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var code = context.GetOption<CodeOption>();
                    var license = await client.ListLicensesAsync(cancellationToken).FirstOrDefaultAsync(l => l.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
                    if (license is null)
                    {
                        CM.WriteError($"License {code} not found.");
                        return -1;
                    }

                    var type = context.GetOption<TypeOption>();
                    var value = context.GetOption<ValueOption>();

                    List<string>? spdx = null;
                    List<string>? urls = null;
                    List<string>? names = null;
                    List<string>? purls = null;

                    switch (type.ToLowerInvariant())
                    {
                        case "spdx":
                            spdx = withValue(license.Spdx, value);
                            break;

                        case "url":
                            urls = withValue(license.Urls, readUrl(value));
                            break;

                        case "packagename":
                            names = withValue(license.PackageNames, value);
                            break;

                        case "purl":
                            purls = withValue(license.Purls, value);
                            break;

                        default:
                            // this should already be handled by the ValidValues check which will write a better error message
                            CM.WriteError<TypeOption>("Invalid type.");
                            return -1;
                    }

                    await client.UpdateLicenseAsync(
                        new LicenseInfo
                        {
                            Id = license.Id,
                            Title = license.Title,
                            Code = license.Code,
                            Spdx = spdx,
                            Urls = urls,
                            PackageNames = names,
                            Purls = purls,
                            Hashes = license.Hashes
                        },
                        cancellationToken
                    );

                    Console.WriteLine("License detection updated.");

                    return 0;

                    static string readUrl(string url)
                    {
                        var m = UrlRegex().Match(url);
                        if (m.Success)
                            return m.Groups[1].Value;
                        else
                            return url;
                    }

                    static List<string>? withValue(IReadOnlyList<string>? list, string value)
                    {
                        if (list is null || list.Count == 0)
                            return [value];

                        return [.. list.Union([value], StringComparer.OrdinalIgnoreCase)];
                    }
                }

                [GeneratedRegex("^https?://(?<1>.+)$", RegexOptions.ExplicitCapture)]
                private static partial Regex UrlRegex();
            }
        }
    }
}
