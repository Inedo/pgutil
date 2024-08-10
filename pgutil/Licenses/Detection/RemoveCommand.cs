using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class LicensesCommand
    {
        private sealed partial class DetectionCommand
        {
            private sealed class RemoveCommand : IConsoleCommand
            {
                public static string Name => "remove";
                public static string Description => "Removes a detection type from a license";
                public static string Examples => """
                  $> pgutil licenses detection remove --code=ABC-1.0 --type=purl --value=pkg:nuget/myNugetPackage@1.2.3

                  $> pgutil licenses detection remove --code=XYZ-2.0 --type=spdx --value=MIT

                  $> pgutil licenses detection remove --code=NewLicense --type=url --value=https://proget.corp.local

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-licenses/proget-api-licenses-update
                """;

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
                            spdx = withoutValue(license.Spdx, value);
                            break;

                        case "url":
                            urls = withoutValue(license.Urls, value);
                            break;

                        case "packagename":
                            names = withoutValue(license.PackageNames, value);
                            break;

                        case "purl":
                            purls = withoutValue(license.Purls, value);
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

                    static List<string>? withoutValue(IReadOnlyList<string>? list, string value)
                    {
                        if (list is null || list.Count == 0)
                            return [value];

                        return [.. list.Except([value], StringComparer.OrdinalIgnoreCase)];
                    }
                }
            }
        }
    }
}
