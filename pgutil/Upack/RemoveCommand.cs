using ConsoleMan;
using Inedo.ProGet.UniversalPackages;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class UpackCommand
    {
        private sealed class RemoveCommand : IConsoleCommand
        {
            public static string Name => "remove";
            public static string Description => "Removes an installed universal package";
            public static string Examples => """
                  $> pgutil upack remove --package=my-package

                For more information, see: https://docs.inedo.com/docs/proget/feeds/universal#installing-universal-packages
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var fullName = context.GetOption<PackageNameOption>();
                var (group, name) = ParseName(fullName);

                using var registry = UniversalPackageRegistry.GetRegistry(true);
                var package = registry.GetInstalledPackages()
                    .FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(group ?? string.Empty, p.Group ?? string.Empty, StringComparison.OrdinalIgnoreCase));

                if (package is null)
                {
                    CM.WriteError($"Package {fullName} is not installed.");
                    return -1;
                }

                await RemoveAsync(package, registry, cancellationToken);
                return 0;
            }
        }
    }
}
