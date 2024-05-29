namespace ConsoleMan;

public sealed record class OptionOverrides(bool? Required = null, string? Description = null, string[]? ValidValues = null, string? DefaultValue = null);