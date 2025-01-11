using System.IO.Compression;
using ConsoleMan;
using Inedo.ProGet;
using Inedo.ProGet.UniversalPackages;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class UpackCommand : IConsoleCommandContainer
    {
        public static string Name => "upack";
        public static string Description => "Work with ProGet universal packages";

        public static void Configure(ICommandBuilder builder)
        {
            builder.WithCommand<CreateCommand>()
                .WithCommand<InstallCommand>()
                .WithCommand<RemoveCommand>()
                .WithCommand<UpdateCommand>()
                .WithCommand<ListCommand>();
        }

        private static async Task InstallAsync(CommandContext context, UniversalPackageInfo pid, string targetDirectory, UniversalPackageRegistry registry, bool overwrite, CancellationToken cancellationToken)
        {
            var client = context.GetProGetClient();

            using var tempPackageStream = new FileStream(Path.GetTempFileName(), new FileStreamOptions { Access = FileAccess.ReadWrite, Mode = FileMode.Create, Options = FileOptions.DeleteOnClose });

            CM.WriteLine($"Downloading {pid.Name}...");
            using (var downloadStream = await client.DownloadUniversalPackageAsync(context.GetFeedName(), pid.Group, pid.Name, pid.Version, cancellationToken))
            {
                await downloadStream.CopyToAsync(tempPackageStream, cancellationToken);
            }

            tempPackageStream.Position = 0;

            targetDirectory = Path.GetFullPath(targetDirectory);

            CM.WriteLine($"Installing to {targetDirectory}...");
            ExtractItems(tempPackageStream, targetDirectory, overwrite);

            CM.WriteLine("Registering package...");
            await registry.LockAsync($"pgutil upack install {pid.Name}", cancellationToken);
            registry.RegisterPackage(
                new RegisteredUniversalPackage
                {
                    Name = pid.Name,
                    Group = pid.Group,
                    Path = targetDirectory,
                    Version = pid.Version,
                    InstallationDate = DateTimeOffset.Now.ToString("o"),
                    InstalledBy = Environment.UserName,
                    InstalledUsing = "pgutil/" + typeof(Program).Assembly.GetName().Version?.ToString()
                }
            );
            registry.Unlock();

            CM.WriteLine("Installation complete.");

            static void ExtractItems(Stream packgeStream, string targetPath, bool overwrite)
            {
                var dirs = new HashSet<string>();

                using var zip = new ZipArchive(packgeStream, ZipArchiveMode.Read, true);
                foreach (var entry in zip.Entries)
                {
                    if (!entry.FullName.StartsWith("package/", StringComparison.OrdinalIgnoreCase) && !entry.FullName.StartsWith("package\\", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var targetEntryPath = Path.Combine(targetPath, entry.FullName["package/".Length..].Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
                    var parentDir = Path.GetDirectoryName(targetEntryPath)!;
                    if (dirs.Add(parentDir))
                        Directory.CreateDirectory(parentDir);

                    try
                    {
                        entry.ExtractToFile(targetEntryPath, overwrite);
                    }
                    catch (IOException) when (File.Exists(targetEntryPath))
                    {
                        throw new PgUtilException($"{targetEntryPath} already exists and --overwrite was not specified");
                    }
                }
            }
        }
        private static async Task RemoveAsync(RegisteredUniversalPackage package, UniversalPackageRegistry registry, CancellationToken cancellationToken)
        {
            CM.WriteLine("Removing package from registry...");
            await registry.LockAsync($"pgutil upack remove {package.Name}", cancellationToken);
            registry.UnregisterPackage(package);
            registry.Unlock();

            CM.WriteLine($"Deleting content of {package.Path}...");
            var installDir = new DirectoryInfo(package.Path);
            if (installDir.Exists)
            {
                foreach (var item in installDir.GetFileSystemInfos())
                {
                    if (item is DirectoryInfo d)
                        d.Delete(true);
                    else
                        item.Delete();
                }
            }

            CM.WriteLine("Package removed.");
        }
        private static async Task<UniversalPackageInfo?> GetPackageAsync(CommandContext context, CancellationToken cancellationToken)
        {
            var fullName = context.GetOption<PackageNameOption>();
            var (group, name) = ParseName(fullName);

            var version = context.GetOption<PackageVersionOption>();
            UniversalPackageVersion? parsedVersion = null;
            bool latest = false;
            bool latestUnstable = false;
            switch (version)
            {
                case "latest":
                    latest = true;
                    break;

                case "latest-unstable":
                    latestUnstable = true;
                    break;

                default:
                    parsedVersion = UniversalPackageVersion.Parse(version);
                    break;
            }

            var client = context.GetProGetClient();

            UniversalPackageInfo? packageInfo = null;
            var feedName = context.GetFeedName();

            await foreach (var p in client.ListUniversalPackageVersions(feedName, group ?? string.Empty, name, cancellationToken))
            {
                var otherParsedVersion = UniversalPackageVersion.Parse(p.Version);
                if (latest)
                {
                    if (string.IsNullOrEmpty(otherParsedVersion.Prerelease))
                    {
                        if (packageInfo is null || UniversalPackageVersion.Parse(packageInfo.Version) < otherParsedVersion)
                            packageInfo = p;
                    }
                }
                else if (latestUnstable)
                {
                    if (packageInfo is null || UniversalPackageVersion.Parse(packageInfo.Version) < otherParsedVersion)
                        packageInfo = p;
                }
                else if (parsedVersion == otherParsedVersion)
                {
                    packageInfo = p;
                    break;
                }
            }

            return packageInfo;
        }
        private static (string? group, string name) ParseName(string fullName)
        {
            var name = fullName;
            string? group = null;
            int index = fullName.LastIndexOf('/');
            if (index >= 0)
            {
                group = fullName[..index];
                name = fullName[(index + 1)..];
            }

            return (group, name);
        }

        private sealed class PackageNameOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--package";
            public static string Description => "Name (and group if applicable) of package";
        }

        private sealed class PackageVersionOption : IConsoleOption
        {
            public static bool Required => true;
            public static string Name => "--version";
            public static string Description => "Version of package. May be a specific version, latest, or latest-unstable";
            public static string DefaultValue => "latest";
        }
    }
}
