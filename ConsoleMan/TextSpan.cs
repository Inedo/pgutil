namespace ConsoleMan;

public readonly record struct TextSpan(string? Text, ConsoleColor? Color = null)
{
    public static implicit operator TextSpan(string? s) => new(s);
}
