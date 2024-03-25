using System.Buffers;
using System.Runtime.CompilerServices;

namespace ConsoleMan;

public sealed class MessageWriter(int length)
{
    private readonly char[] buffer = new char[length];
    private int length;

    internal ReadOnlySpan<char> Value => this.buffer.AsSpan(0, length);

    private Span<char> Remaining => this.buffer.AsSpan(this.length);

    public void Write(string? s)
    {
        var r = this.Remaining;

        if (r.IsEmpty || s is null)
            return;

        if (!s.TryCopyTo(r))
        {
            s.AsSpan(0, r.Length).CopyTo(r);
            this.length = this.buffer.Length;
        }
        else
        {
            this.length += s.Length;
        }
    }
    public void Write(ReadOnlySpan<char> value)
    {
        var r = this.Remaining;
        if (r.IsEmpty)
            return;

        if (!value.TryCopyTo(r))
        {
            value[..r.Length].CopyTo(r);
            this.length = this.buffer.Length;
        }
        else
        {
            this.length += value.Length;
        }
    }
    public void Write<T>(T value, string? format = null) where T : ISpanFormattable
    {
        var r = this.Remaining;

        if (r.IsEmpty || value is null)
            return;

        if (!value.TryFormat(r, out int charsWritten, format, null))
        {
            var temp = ArrayPool<char>.Shared.Rent(256);
            if (!value.TryFormat(temp, out charsWritten, format, null))
            {
                ArrayPool<char>.Shared.Return(temp);
                // just give up if it's that big
                this.length = this.buffer.Length;
                return;
            }

            int len = Math.Min(r.Length, charsWritten);
            temp.AsSpan(0, len).CopyTo(r);
            this.length += len;
        }
        else
        {
            this.length += charsWritten;
        }
    }
    public void Write(object? value) => this.Write(value?.ToString());
#pragma warning disable IDE0060 // Remove unused parameter - parameter is used by compiler
#pragma warning disable CA1822 // Mark members as static - cannot be static because it's used by an interpolated string handler
    public void Write([InterpolatedStringHandlerArgument("")] ref FixedLengthInterpolatedStringHandler value)
#pragma warning restore CA1822 // Mark members as static
#pragma warning restore IDE0060 // Remove unused parameter
    {
    }
    public void WriteSize(long value)
    {
        if (value == 0)
            this.Write("0");
        else if (value < 1024)
            this.Write($"{value:N0}b");
        else if (value < 1024 * 1024)
            this.Write($"{value / 1024:N0}kb");
        else if (value < 1024 * 1024 * 1024)
            this.Write($"{value / (1024 * 1024):N0}mb");
        else
            this.Write($"{value / (1024 * 1024 * 1024):N0}gb");
    }

    internal void Reset() => this.length = 0;

    [InterpolatedStringHandler]
    public readonly struct FixedLengthInterpolatedStringHandler
    {
        private readonly MessageWriter messageWriter;

#pragma warning disable IDE0060 // Remove unused parameter - parameters are required for interpolated string handler pattern
#pragma warning disable IDE0290 // Use primary constructor
        public FixedLengthInterpolatedStringHandler(int literalLength, int formattedCount, MessageWriter messageWriter) => this.messageWriter = messageWriter;
#pragma warning restore IDE0290 // Use primary constructor
#pragma warning restore IDE0060 // Remove unused parameter

        public void AppendLiteral(string s) => this.messageWriter.Write(s);
        public void AppendFormatted(ReadOnlySpan<char> value) => this.messageWriter.Write(value);
        public void AppendFormatted<T>(T value, string? format = null) where T : ISpanFormattable => this.messageWriter.Write(value, format);
        public void AppendFormatted(object? value) => this.messageWriter.Write(value);
    }
}
