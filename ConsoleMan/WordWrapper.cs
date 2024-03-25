namespace ConsoleMan;

internal static class WordWrapper
{
    public static void WriteOutput(string? text, int margin = 0)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (Console.IsOutputRedirected)
            Console.Write(text);
        else
            Write(Console.Out, text, margin);
    }
    public static void WriteError(string? text, int margin = 0)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (Console.IsErrorRedirected)
            Console.Error.Write(text);
        else
            Write(Console.Error, text, margin);
    }

    private static void Write(TextWriter writer, string text, int margin)
    {
        bool first = true;

        foreach (var line in text.Split('\n'))
        {
            if (first)
            {
                first = false;
            }
            else
            {
                writer.WriteLine();
                for (int i = 0; i < margin; i++)
                    writer.Write(' ');
            }

            writeLine(line.Trim('\r'));
        }
        
        void writeLine(string text)
        {
            var (start, _) = Console.GetCursorPosition();
            int width = Console.BufferWidth;

            // for absurdly large margins, don't even try to wrap
            if (margin >= width - 5)
                writer.Write(text);

            var remainder = text.AsSpan().Trim();

            while (!remainder.IsEmpty)
            {
                int lineWidth = width - start;
                if (remainder.Length < lineWidth)
                {
                    writer.Write(remainder);
                    break;
                }
                else
                {
                    var linePart = remainder[..lineWidth];
                    int splitIndex = linePart.LastIndexOf(' ');
                    if (splitIndex < 0)
                        splitIndex = linePart.LastIndexOfAny("-,;/:");

                    if (splitIndex > 0)
                    {
                        writer.Write(linePart[..splitIndex]);
                        remainder = remainder[splitIndex..];
                    }
                    else
                    {
                        writer.Write(linePart[..lineWidth]);
                        remainder = remainder[lineWidth..];
                    }

                    remainder = remainder.TrimStart();

                    writer.WriteLine();
                    for (int i = 0; i < margin; i++)
                        writer.Write(' ');

                    start = margin;
                }
            }
        }
    }
}
