using ConsoleMan;
using Inedo.ProGet.UniversalPackages;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class UpackCommand
    {
        private sealed class UpdateCommand : IConsoleCommand
        {
            public static string Name => "update";
            public static string Description => "Updates an installed package";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<FeedOption>()
                    .WithOption<PackageNameOption>()
                    .WithOption<PackageVersionOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var fullName = context.GetOption<PackageNameOption>();
                var (group, name) = ParseName(fullName);

                using var registry = UniversalPackageRegistry.GetRegistry(false);
                var package = registry.GetInstalledPackages()
                    .FirstOrDefault(p => string.Equals(name, p.Name, StringComparison.OrdinalIgnoreCase) && string.Equals(group ?? string.Empty, p.Group ?? string.Empty, StringComparison.OrdinalIgnoreCase));

                if (package is null)
                {
                    CM.WriteError($"Package {fullName} is not installed.");
                    return -1;
                }

                var pid = await GetPackageAsync(context, cancellationToken) ?? throw new PgUtilException("Package not found.");

                if (UniversalPackageVersion.Parse(package.Version) == UniversalPackageVersion.Parse(pid.Version))
                {
                    CM.WriteLine($"Package {fullName} {package.Version} is already installed.");
                    return 0;
                }

                await RemoveAsync(package, registry, cancellationToken);
                await InstallAsync(context, pid, package.Path, registry, true, cancellationToken);
                return 0;
            }
        }
    }
}
