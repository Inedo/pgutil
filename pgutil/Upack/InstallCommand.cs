using ConsoleMan;
using Inedo.ProGet.UniversalPackages;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class UpackCommand
    {
        private sealed class InstallCommand : IConsoleCommand
        {
            public static string Name => "install";
            public static string Description => "Installs a universal package from a feed";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<FeedOption>()
                    .WithOption<PackageNameOption>()
                    .WithOption<PackageVersionOption>()
                    .WithOption<DoNotRegisterFlag>()
                    .WithOption<TargetDirectoryOption>()
                    .WithOption<OverwriteFlag>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var fullName = context.GetOption<PackageNameOption>();
                var (group, name) = ParseName(fullName);

                using var registry = UniversalPackageRegistry.GetRegistry(true);
                if (registry.GetInstalledPackages().Any(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) && string.Equals(p.Group ?? string.Empty, group ?? string.Empty, StringComparison.OrdinalIgnoreCase)))
                {
                    CM.WriteError($"Package {fullName} is already installed.");
                    return -1;
                }

                var pid = await GetPackageAsync(context, cancellationToken) ?? throw new PgUtilException("Package not found.");
                await InstallAsync(context, pid, context.GetOption<TargetDirectoryOption>(), registry, context.HasFlag<OverwriteFlag>(), cancellationToken);
                return 0;
            }

            private sealed class DoNotRegisterFlag : IConsoleFlagOption
            {
                public static string Name => "--do-not-register";
                public static string Description => "Do not add registration information to the local registry";
            }

            private sealed class TargetDirectoryOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--target";
                public static string Description => "Directory where the contents of the package will be extracted";
            }

            private sealed class OverwriteFlag : IConsoleFlagOption
            {
                public static string Name => "--overwrite";
                public static string Description => "Overwrite files in target directory";
            }
        }
    }
}
