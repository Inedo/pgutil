namespace ConsoleMan;

public static class CM
{
    public static void Write(ConsoleColor color, string? s)
    {
        var current = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(s);
        Console.ForegroundColor = current;
    }
    public static void WriteLine(ConsoleColor color, string? s)
    {
        Write(color, s);
        Console.WriteLine();
    }
    public static void WriteError(string? s)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(s);
        Console.ForegroundColor = color;
    }
    public static void WriteError<TOption>(string? s) where TOption : IConsoleOption
    {
        WriteError($"{TOption.Name} error: {s}");
    }

    public static void Write(params TextSpan[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        foreach (var (text, color) in values)
        {
            if (color.HasValue)
                Write(color.GetValueOrDefault(), text);
            else
                Console.Write(text);
        }
    }

    public static void WriteLine() => Console.WriteLine();
    public static void WriteLine(params TextSpan[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        Write(values);
        Console.WriteLine();
    }
    public static void WriteTwoColumnList(params (string Key, string Value)[] items) => WriteTwoColumnList(items, null);
    public static void WriteTwoColumnList(IEnumerable<(string Key, string Value)> items, int? margin = null)
    {
        margin ??= items.Select(i => i.Key.Length).Max() + 2;
        foreach (var (Key, Value) in items)
        {
            Console.Write(Key);
            for (int i = Key.Length; i < margin; i++)
                Console.Write(' ');

            WordWrapper.WriteOutput(Value, margin.Value);
            Console.WriteLine();
        }
    }
}
