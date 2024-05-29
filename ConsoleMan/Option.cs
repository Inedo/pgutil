namespace ConsoleMan;

public abstract class Option : ConsoleToken
{
    private protected Option()
    {
    }

    public abstract bool Required { get; }
    public abstract string[]? ValidValues { get; }
    public abstract string? DefaultValue { get; }
    public abstract bool HasValue { get; }

    internal bool TryParse(string arg, out string? value)
    {
        value = null;
        var name = this.Name;

        if (arg.StartsWith(name))
        {
            if (arg.Length == name.Length)
            {
                return true;
            }
            else if (arg[name.Length] == '=')
            {
                value = arg[(name.Length + 1)..];
                return true;
            }
        }

        return false;
    }
}

internal sealed class Option<TOption>(OptionOverrides? o = null) : Option where TOption : IConsoleOption
{
    private readonly OptionOverrides? o = o;

    public override bool Required => o?.Required ?? TOption.Required;
    public override string Name => TOption.Name;
    public override string Description => o?.Description ?? TOption.Description;
    public override Type Type => typeof(TOption);
    public override string[]? ValidValues => o?.ValidValues ?? TOption.ValidValues;
    public override string? DefaultValue => o?.DefaultValue ?? TOption.DefaultValue;
    public override bool HasValue => TOption.HasValue;
}
