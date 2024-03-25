namespace Inedo.DependencyScan;

/// <summary>
/// Contains information about a package recognized as a dependency.
/// </summary>
public sealed class DependencyPackage : IEquatable<DependencyPackage>
{
    internal DependencyPackage()
    {
    }

    /// <summary>
    /// Gets the package name.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// Gets the package group if applicable; otherwise null.
    /// </summary>
    public string? Group { get; init; }
    /// <summary>
    /// Gets the package version.
    /// </summary>
    public required string Version { get; init; }
    /// <summary>
    /// Gets the package type.
    /// </summary>
    public string? Type { get; init; }
    /// <summary>
    /// Gets the package qualifier.
    /// </summary>
    public string? Qualifier { get; init; }

    public override int GetHashCode() => HashCode.Combine(this.Name ?? string.Empty, this.Group ?? string.Empty, this.Version ?? string.Empty);
    public bool Equals(DependencyPackage? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;

        return this.Group == other.Group && this.Name == other.Name && this.Version == other.Version;
    }
    public override bool Equals(object? obj) => this.Equals(obj as DependencyPackage);
    public override string ToString()
    {
        var name = string.IsNullOrWhiteSpace(this.Group) ? this.Name : (this.Group + "/" + this.Name);
        return $"{name} {this.Version}";
    }
}
