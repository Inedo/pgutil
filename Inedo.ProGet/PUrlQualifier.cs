using System.Collections;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Inedo.ProGet;

/// <summary>
/// Represents the qualifier portion of a PackageUrl.
/// </summary>
/// <remarks>
/// This is essentially a URL query string with some additional rules:
/// <list type="bullet">
/// <item>Keys must not be duplicated. A duplicate key will generate a <see cref="FormatException"/> when <see cref="Normalize"/> is called.</item>
/// <item>Keys must have values. A value may be empty, but a string of the format <c>key1&amp;key2=value</c> is not value. Missing values are silently converted to empty strings by <see cref="Normalize"/>.</item>
/// <item>Keys must be in alphabetical order. This is corrected by <see cref="Normalize"/>.</item>
/// </list>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="PUrlQualifier"/> struct.
/// </remarks>
/// <param name="qualifier">The raw qualifier string.</param>
public readonly struct PUrlQualifier(string? qualifier) : IReadOnlyDictionary<string, string>, IEquatable<PUrlQualifier>, IComparable<PUrlQualifier>, IComparable
{
    private readonly string? q = qualifier == string.Empty ? null : qualifier;

    public static implicit operator string?(PUrlQualifier q) => !q.IsEmpty ? q.q : null;
    public static bool operator ==(PUrlQualifier q1, PUrlQualifier q2) => q1.Equals(q2);
    public static bool operator !=(PUrlQualifier q1, PUrlQualifier q2) => !q1.Equals(q2);

    /// <summary>
    /// Gets the value of the specified key, or an empty string if not found.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The value of the key if found; otherwise empty string.</returns>
    /// <remarks>
    /// This is provided for convenience. Prefer using <see cref="HasValue(string, ReadOnlySpan{char}, StringComparison)"/> or
    /// <see cref="TryGetValue(string, out ReadOnlySpan{char})"/> instead to prevent extra allocations.
    /// </remarks>
    public string this[string key] => this.TryGetValue(key, out string? v) ? v : string.Empty;

    IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => this.Select(p => p.Key);
    IEnumerable<string> IReadOnlyDictionary<string, string>.Values => this.Select(p => p.Value);

    /// <summary>
    /// Gets a value indicating whether the qualifier is empty.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(this.q);
    /// <summary>
    /// Gets the number of keys in the qualifier.
    /// </summary>
    public int Count
    {
        get
        {
            if (string.IsNullOrEmpty(this.q))
                return 0;

            int count = 0;
            var iterator = this.CreateIterator();
            while (iterator.MoveNext())
            {
                count++;
            }

            return count;
        }
    }

    /// <summary>
    /// Returns a normalized version of this <see cref="PUrlQualifier"/>.
    /// </summary>
    /// <returns>Normalized version of this <see cref="PUrlQualifier"/>.</returns>
    /// <remarks>
    /// This performs validation and will sort keys if necessary. See Remarks on <see cref="PUrlQualifier"/>.
    /// </remarks>
    /// <exception cref="FormatException">The value could not be normalized.</exception>
    public PUrlQualifier Normalize()
    {
        if (string.IsNullOrEmpty(this.q))
            return default;

        var iterator = this.CreateIterator();
        if (!iterator.MoveNext())
            return default;

        var lastKey = iterator.CurrentKey;
        bool outOfOrder = false;

        while (iterator.MoveNext())
        {
            int res = lastKey.CompareTo(iterator.CurrentKey, StringComparison.OrdinalIgnoreCase);
            if (res == 0)
            {
                throw new FormatException($"Invalid purl: Duplicate key \"{lastKey}\" in qualifier.");
            }
            else if (res > 0)
            {
                outOfOrder = true;
                break;
            }
        }

        if (!outOfOrder)
            return this;

        // list is out of order, so we have to sort and rebuild
        // this could be done without so many allocations, but hopefully will be a rare case
        iterator = this.CreateIterator();
        var list = new List<KeyValuePair<string, string>>();
        while (iterator.MoveNext())
        {
            list.Add(new(iterator.CurrentKey.ToString(), iterator.CurrentValue.ToString()));
        }

        list.Sort((p1, p2) => string.Compare(p1.Key, p2.Key, StringComparison.OrdinalIgnoreCase));

        return new PUrlQualifier(string.Join('&', list.Select(i => $"{i.Key}={i.Value}")));
    }

    public bool ContainsKey(string key) => this.TryGetValue(key, out ReadOnlySpan<char> _);
    public bool TryGetValue(string key, out ReadOnlySpan<char> value)
    {
        value = default;

        if (string.IsNullOrEmpty(this.q))
            return false;

        var iterator = this.CreateIterator();
        while (iterator.MoveNext())
        {
            if (iterator.CurrentKeyEquals(key))
            {
                value = iterator.CurrentUnescapedValue();
                return true;
            }
        }

        return false;
    }
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        if (this.TryGetValue(key, out ReadOnlySpan<char> v))
        {
            value = v.ToString();
            return true;
        }

        value = null;
        return false;
    }
    public bool HasValue(string key, ReadOnlySpan<char> value, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        return this.TryGetValue(key, out ReadOnlySpan<char> v) && v.Equals(value, comparisonType);
    }

    public int CompareTo(PUrlQualifier other) => string.Compare(this.q, other.q, StringComparison.OrdinalIgnoreCase);
    public bool Equals(PUrlQualifier other) => string.Equals(this.q, other.q, StringComparison.OrdinalIgnoreCase);
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is PUrlQualifier q && this.Equals(q);
    public override int GetHashCode() => this.q?.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
    public override string ToString() => (string?)this ?? string.Empty;
    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        if (this.IsEmpty)
            return Enumerable.Empty<KeyValuePair<string, string>>().GetEnumerator();

        var list = new List<KeyValuePair<string, string>>();
        var iterator = this.CreateIterator();
        while (iterator.MoveNext())
        {
            list.Add(new(iterator.CurrentUnescapedKey().ToString(), iterator.CurrentUnescapedValue().ToString()));
        }

        return list.GetEnumerator();
    }

    private Iterator CreateIterator() => new(this.q);

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    int IComparable.CompareTo(object? obj) => obj is PUrlQualifier other ? this.CompareTo(other) : throw new ArgumentException($"Expected {nameof(PUrlQualifier)}.");

    private ref struct Iterator
    {
        private ReadOnlySpan<char> remainder;

        public Iterator(ReadOnlySpan<char> qualifier)
        {
            this.remainder = qualifier;
            this.CurrentKey = default;
            this.CurrentValue = default;
        }

        public ReadOnlySpan<char> CurrentKey { get; private set; }
        public ReadOnlySpan<char> CurrentValue { get; private set; }

        public readonly bool CurrentKeyEquals(ReadOnlySpan<char> other) => this.CurrentUnescapedKey().Equals(other, StringComparison.OrdinalIgnoreCase);
        public readonly ReadOnlySpan<char> CurrentUnescapedKey()
        {
            if (!this.CurrentKey.Contains('%'))
                return this.CurrentKey;
            else
                return Uri.UnescapeDataString(this.CurrentKey.ToString());
        }
        public readonly ReadOnlySpan<char> CurrentUnescapedValue()
        {
            if (!this.CurrentValue.Contains('%'))
                return this.CurrentValue;
            else
                return Uri.UnescapeDataString(this.CurrentValue.ToString());
        }
        public bool MoveNext()
        {
            if (this.remainder.IsEmpty)
            {
                this.CurrentKey = default;
                this.CurrentValue = default;
                return false;
            }

            int ampIndex = this.remainder.IndexOf('&');
            var currentPair = ampIndex >= 0 ? this.remainder[0..ampIndex] : this.remainder;

            int equalsIndex = currentPair.IndexOf('=');
            if (equalsIndex >= 0)
            {
                this.CurrentKey = currentPair[0..equalsIndex];
                this.CurrentValue = equalsIndex < currentPair.Length ? currentPair[(equalsIndex + 1)..] : default;
            }
            else
            {
                this.CurrentKey = currentPair;
                this.CurrentValue = default;
            }

            this.remainder = this.remainder[currentPair.Length..].TrimStart('&');
            return true;
        }
    }
}
