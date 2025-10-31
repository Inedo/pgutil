namespace Inedo.ProGet;

public sealed class SecurityUser
{
    public required string Name { get; init; }
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
    public string[]? Groups { get; init; }
}
