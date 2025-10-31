namespace Inedo.ProGet;

public sealed class SecurityGroup
{
    public required string Name { get; init; }
    public string[]? Users { get; init; }
}
