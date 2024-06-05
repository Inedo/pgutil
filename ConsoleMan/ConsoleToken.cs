namespace ConsoleMan;

public abstract class ConsoleToken : IEquatable<ConsoleToken>
{
    private protected ConsoleToken()
    {
    }

    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Type Type { get; }
    public abstract bool Undisclosed { get; }

    public bool Equals(ConsoleToken? other)
    {
        if (ReferenceEquals(this, other))
            return true;
        if (other is null)
            return false;

        return this.Type == other.Type;
    }
    public override bool Equals(object? obj) => this.Equals(obj as ConsoleToken);
    public override int GetHashCode() => this.Type.GetHashCode();
    public override string ToString() => this.Name;
}
