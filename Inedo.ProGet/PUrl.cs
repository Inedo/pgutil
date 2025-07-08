using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

#nullable enable

namespace Inedo.ProGet;

/// <summary>
/// Represents a PackageUrl, as defined by https://github.com/package-url/purl-spec.
/// </summary>
public sealed partial class PUrl : IEquatable<PUrl>
{
    public PUrl(string packageType, string packageName, string packageVersion, string? packageGroupName = null, string? qualifier = null, int? packageId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(packageType);
        ArgumentException.ThrowIfNullOrEmpty(packageName);
        ArgumentException.ThrowIfNullOrEmpty(packageVersion);

        this.PackageType = packageType;
        this.PackageName = packageName;
        this.PackageVersion = packageVersion;
        this.PackageGroupName = packageGroupName;
        this.Qualifier = new PUrlQualifier(qualifier).Normalize();
        this.PackageId = packageId;
    }

    public static bool operator ==(PUrl? p1, PUrl? p2) => Equals(p1, p2);
    public static bool operator !=(PUrl? p1, PUrl? p2) => !Equals(p1, p2);

    /// <summary>
    /// Gets the package type.
    /// </summary>
    /// <remarks>
    /// This is always lowercase, and does not correspond exactly to ProGet's feed type names.
    /// </remarks>
    public string PackageType { get; }
    /// <summary>
    /// Gets the package name.
    /// </summary>
    public string PackageName { get; }
    /// <summary>
    /// Gets the package version.
    /// </summary>
    public string PackageVersion { get; }
    /// <summary>
    /// Gets the package group, or <c>null</c> if not applicable.
    /// </summary>
    public string? PackageGroupName { get; }
    /// <summary>
    /// Gets the package qualifier string, or <c>null</c> if not applicable.
    /// </summary>
    public PUrlQualifier Qualifier { get; }
    /// <summary>
    /// Gets the internal database ID for the package if available.
    /// </summary>
    public int? PackageId { get; }
    /// <summary>
    /// Gets a valud indicating whether the <see cref="PackageName"/> and <see cref="PackageGroupName"/> are treated as case-sensitive when comparing PUrls.
    /// </summary>
    public bool CaseSensitive => this.PackageType == "rpm";
    /// <summary>
    /// Gets the package group and name joined by a / character.
    /// </summary>
    public string GroupAndName => string.IsNullOrEmpty(this.PackageGroupName) ? this.PackageName : $"{this.PackageGroupName}/{this.PackageName}";

    public static PUrl Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentNullException(nameof(s));

        if (!TryParse(s, out var purl))
            throw new FormatException("Invalid format for purl string.");

        return purl;
    }
    public static bool TryParse(string? s, [NotNullWhen(true)] out PUrl? purl)
    {
        purl = null;
        if (string.IsNullOrWhiteSpace(s))
            return false;

        var m = PUrlRegex().Match(s);
        if (!m.Success)
            return false;

        var type = m.Groups[1].Value;

        var fullName = m.Groups[2].ValueSpan.Trim('/');
        int nameStartIndex = fullName.LastIndexOf('/');

        string? group;
        string name;

        if (nameStartIndex > 0)
        {
            group = Uri.UnescapeDataString(fullName[..nameStartIndex].ToString());
            name = Uri.UnescapeDataString(fullName[(nameStartIndex + 1)..].ToString());
        }
        else
        {
            group = null;
            name = fullName.ToString();
        }

        var version = Uri.UnescapeDataString(m.Groups[3].Value);
        var qualifierGroup = m.Groups[4];
        var qualifier = qualifierGroup.Success ? qualifierGroup.Value : null;

        purl = new PUrl(type, name, version, group, qualifier);
        return true;
    }
    public static bool Equals(PUrl? p1, PUrl? p2)
    {
        if (ReferenceEquals(p1, p2))
            return true;
        if (p1 is null || p2 is null)
            return false;

        // these are always made canonical and lowercase
        if (p1.PackageType != p2.PackageType)
            return false;

        var compare = p1.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (!string.Equals(p1.PackageName, p2.PackageName, compare) || !string.Equals(p1.PackageGroupName, p2.PackageGroupName, compare))
            return false;

        if (!string.Equals(p1.PackageVersion, p2.PackageVersion, StringComparison.OrdinalIgnoreCase))
            return false;

        return p1.Qualifier == p2.Qualifier;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("pkg:");
        sb.Append(this.PackageType);
        sb.Append('/');
        if (!string.IsNullOrEmpty(this.PackageGroupName))
        {
            sb.Append(EscapeUrlString(this.PackageGroupName));
            sb.Append('/');
        }

        sb.Append(Uri.EscapeDataString(this.PackageName));
        sb.Append('@');
        sb.Append(Uri.EscapeDataString(this.PackageVersion));

        if (!string.IsNullOrEmpty(this.Qualifier))
        {
            sb.Append('?');
            sb.Append(this.Qualifier);
        }

        return sb.ToString();
    }
    public bool Equals(PUrl? other) => Equals(this, other);
    public override bool Equals(object? obj) => this.Equals(obj as PUrl);
    public override int GetHashCode()
    {
        var compare = this.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        return HashCode.Combine(
            this.PackageType,
            this.PackageName.GetHashCode(compare),
            this.PackageGroupName?.GetHashCode(compare) ?? 0,
            this.PackageVersion.GetHashCode(StringComparison.OrdinalIgnoreCase),
            this.Qualifier.GetHashCode()
        );
    }

    private static string EscapeUrlString(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        if (!s.Contains('/'))
            return Uri.EscapeDataString(s);

        var parts = s.Split('/');
        return string.Join('/', parts.Select(Uri.EscapeDataString));
    }

    [GeneratedRegex(@"^pkg:(?<1>[^/]+)/(?<2>[^@]+)@(?<3>[^\?]+)(\?(?<4>[^#]+))?", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex PUrlRegex();
}
