namespace ConsoleMan;

public interface IConsoleOption : IConsoleArgument
{
    static abstract bool Required { get; }
    static virtual bool HasValue => true;
    static virtual string[]? ValidValues => null;
    static virtual string? DefaultValue => null;
}

public interface IConsoleEnumOption<TEnum> : IConsoleOption where TEnum : struct, Enum
{
    static string[]? IConsoleOption.ValidValues
    {
        get
        {
            var names = Enum.GetNames<TEnum>();
            for (int i = 0; i < names.Length; i++)
                names[i] = names[i].ToLowerInvariant();

            return names;
        }
    }
}
