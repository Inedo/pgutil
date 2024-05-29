namespace ConsoleMan;

public static class CommandBuilderExtensions
{
    public static ICommandBuilder WithOption<TOption>(this ICommandBuilder builder) where TOption : IConsoleOption => builder.WithOption<TOption>(null);
    public static ICommandBuilder WithOption<TOption>(this ICommandBuilder builder, bool required) where TOption : IConsoleOption => builder.WithOption<TOption>(new OptionOverrides(required));
}
