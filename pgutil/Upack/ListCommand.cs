using ConsoleMan;
using Inedo.ProGet.UniversalPackages;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class UpackCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "List packages in the local registry";

            public static void Configure(ICommandBuilder builder)
            {
            }

            public static Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                using var registry = UniversalPackageRegistry.GetRegistry(false);

                var packages = registry.GetInstalledPackages();
                if (packages.Length > 0)
                {
                    foreach (var package in packages)
                    {
                        var fullName = package.Name;
                        if (!string.IsNullOrEmpty(package.Group))
                            fullName = $"{package.Group}/{fullName}";

                        CM.WriteLine($"Name: {fullName}");
                        CM.WriteLine($"Version: {package.Version}");
                        CM.WriteLine($"Install Path: {package.Path}");
                        CM.WriteLine($"Installed On: {package.InstallationDate} by {package.InstalledBy}");
                        CM.WriteLine();
                    }
                }
                else
                {
                    CM.WriteLine("No packages have been registered.");
                }

                return Task.FromResult(0);
            }
        }
    }
}
