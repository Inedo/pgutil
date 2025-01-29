using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Inedo.ProGet.UniversalPackages;

/// <summary>
/// Represents a universal package registry.
/// </summary>
public sealed class UniversalPackageRegistry : IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UniversalPackageRegistry"/> class.
    /// </summary>
    /// <param name="registryRoot">The root directory of the registry. This must be an absolute path.</param>
    /// <exception cref="ArgumentNullException"><paramref name="registryRoot"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="registryRoot"/> is not an absolute path.</exception>
    /// <remarks>
    /// To initialize a <see cref="UniversalPackageRegistry"/> instance with one of the standard registry locations, use the
    /// <see cref="GetRegistry(bool)"/> static method.
    /// </remarks>
    public UniversalPackageRegistry(string registryRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(registryRoot);
        if (!Path.IsPathRooted(registryRoot))
            throw new ArgumentException("Registry root must be an absolute path.");

        this.RegistryRoot = registryRoot;
    }

    /// <summary>
    /// Gets the root directory of the package registry.
    /// </summary>
    public string RegistryRoot { get; }
    /// <summary>
    /// Gets the current lock token if a lock is taken; otherwise <see langword="null"/>.
    /// </summary>
    public string? LockToken { get; private set; }

    /// <summary>
    /// Returns an instance of the <see cref="UniversalPackageRegistry"/> class that represents a package registry on the system.
    /// </summary>
    /// <param name="openUserRegistry">Value indicating whether to open the current user's registry (true) or the machine registry (false).</param>
    /// <returns>Instance of the <see cref="UniversalPackageRegistry"/> class.</returns>
    public static UniversalPackageRegistry GetRegistry(bool openUserRegistry)
    {
        var root = openUserRegistry ? GetCurrentUserRegistryRoot() : GetMachineRegistryRoot();
        return new UniversalPackageRegistry(root);
    }
    /// <summary>
    /// Returns the directory where the machine registry is stored on the current system.
    /// </summary>
    /// <returns>Directory where the machine registry is stored on the current system.</returns>
    public static string GetMachineRegistryRoot() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "upack");
    /// <summary>
    /// Returns the directory where the current user's registry is stored on the current system.
    /// </summary>
    /// <returns>Directory where the current user's registry is stored on the current system.</returns>
    public static string GetCurrentUserRegistryRoot() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".upack");

    /// <summary>
    /// Returns all installed packages in the registry.
    /// </summary>
    /// <returns>Array of installed packages.</returns>
    public RegisteredUniversalPackage[] GetInstalledPackages()
    {
        var fileName = Path.Combine(this.RegistryRoot, "installedPackages.json");

        if (!File.Exists(fileName))
            return [];

        try
        {
            using var configStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            return JsonSerializer.Deserialize(configStream, UniversalPackageJsonContext.Default.RegisteredUniversalPackageArray) ?? [];
        }
        catch (FileNotFoundException)
        {
        }
        catch (JsonException)
        {
        }

        return [];
    }
    public void RegisterPackage(RegisteredUniversalPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        var packages = this.GetInstalledPackages();
        WritePackageList([.. packages.Except([package], new PackageNameComparer()), package]);
    }
    public void UnregisterPackage(RegisteredUniversalPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        var packages = this.GetInstalledPackages();
        WritePackageList([.. packages.Except([package], new PackageNameComparer())]);
    }

    /// <summary>
    /// Acquire exclusive lock of the registry.
    /// </summary>
    /// <param name="reason">Reason for lock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task LockAsync(string? reason = null, CancellationToken cancellationToken = default)
    {
        var fileName = Path.Combine(this.RegistryRoot, ".lock");

        var lockDescription = GetLockReason(reason);
        var lockToken = Guid.NewGuid().ToString();

    TryAgain:
        var fileInfo = getFileInfo();
        while (fileInfo != null && DateTime.UtcNow - fileInfo.LastWriteTimeUtc <= new TimeSpan(0, 0, 10))
        {
            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            fileInfo = getFileInfo();
        }

        // ensure registry root exists
        Directory.CreateDirectory(this.RegistryRoot);

        try
        {
            // write out the lock info
            using (var lockStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(lockStream, new UTF8Encoding(false)))
            {
                writer.WriteLine(lockDescription);
                writer.WriteLine(lockToken.ToString());
            }

            // verify that we acquired the lock
            using (var lockStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(lockStream, Encoding.UTF8))
            {
                if (reader.ReadLine() != lockDescription)
                    goto TryAgain;

                if (reader.ReadLine() != lockToken)
                    goto TryAgain;
            }
        }
        catch (IOException)
        {
            // file may be in use by other process
            goto TryAgain;
        }

        // at this point, lock is acquired provided everyone is following the rules
        this.LockToken = lockToken;

        FileInfo? getFileInfo()
        {
            try
            {
                var info = new FileInfo(fileName);
                if (!info.Exists)
                    return null;
                return info;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }
    }
    /// <summary>
    /// Releases exclusive lock of the registry if it is held.
    /// </summary>
    public void Unlock()
    {
        if (this.LockToken == null)
            return;

        var fileName = Path.Combine(this.RegistryRoot, ".lock");
        if (!File.Exists(fileName))
            return;

        string? token;
        using (var lockStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        using (var reader = new StreamReader(lockStream, Encoding.UTF8))
        {
            reader.ReadLine();
            token = reader.ReadLine();
        }

        if (token == this.LockToken)
            File.Delete(fileName);

        this.LockToken = null;
    }
    /// <inheritdoc/>
    public void Dispose() => this.Unlock();

    private void WritePackageList(RegisteredUniversalPackage[] packages)
    {
        Directory.CreateDirectory(this.RegistryRoot);
        var fileName = Path.Combine(this.RegistryRoot, "installedPackages.json");
        using var stream = File.Create(fileName);
        JsonSerializer.Serialize(stream, packages, UniversalPackageJsonContext.Default.RegisteredUniversalPackageArray);
    }
    private static string GetLockReason(string? reason)
    {
        if (!string.IsNullOrWhiteSpace(reason))
            return reason;

        return "Locked for update";
    }

    private sealed class PackageNameComparer : EqualityComparer<RegisteredUniversalPackage>
    {
        public override bool Equals(RegisteredUniversalPackage? x, RegisteredUniversalPackage? y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;

            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Group ?? string.Empty, y.Group ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
        public override int GetHashCode(RegisteredUniversalPackage obj)
        {
            return HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name),
                StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Group ?? string.Empty)
            );
        }
    }
}
