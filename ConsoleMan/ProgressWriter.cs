using System.Numerics;

namespace ConsoleMan;

public static class ProgressWriter
{
    public static ProgressWriter<TValue> Create<TValue>(TValue minValue, TValue maxValue, FormatStatusMessage<TValue>? getStatusMessage = null) where TValue : notnull, INumber<TValue>
    {
        var (x, y) = Console.GetCursorPosition();
        return new ProgressWriter<TValue>(minValue, maxValue, getStatusMessage, x, y);
    }
}

public sealed class ProgressWriter<TValue> : IDisposable where TValue : notnull, INumber<TValue>
{
    private const int ProgressBarWidth = 20;
    private readonly FormatStatusMessage<TValue>? getStatusMessage;
    private readonly int startColumn;
    private readonly int startRow;
    private readonly double valuePerBlock;
    private readonly MessageWriter statusMessageBuffer1;
    private readonly MessageWriter statusMessageBuffer2;
    private readonly bool wasCursorVisible;
    private bool disposed;

    internal ProgressWriter(TValue minValue, TValue maxValue, FormatStatusMessage<TValue>? getStatusMessage, int startColumn, int startRow)
    {
        this.MinValue = minValue;
        this.MaxValue = maxValue;
        this.CurrentValue = minValue;
        this.getStatusMessage = getStatusMessage;
        this.startColumn = startColumn;
        this.startRow = startRow;
        this.valuePerBlock = 20 / double.CreateTruncating(maxValue - minValue);
        int maxStatusMessageLength = Console.BufferWidth - startColumn - ProgressBarWidth - 5 - 2 - 1;
        this.statusMessageBuffer1 = new MessageWriter(maxStatusMessageLength);
        this.statusMessageBuffer2 = new MessageWriter(maxStatusMessageLength);

        if (OperatingSystem.IsWindows())
        {
            this.wasCursorVisible = Console.CursorVisible;
            Console.CursorVisible = false;
        }

        this.WriteStatus(minValue, true);
    }

    public TValue MinValue { get; }
    public TValue MaxValue { get; }
    public TValue CurrentValue { get; private set; }

    public void SetCurrentValue(TValue value)
    {
        value = TValue.Clamp(value, this.MinValue, this.MaxValue);
        this.WriteStatus(value);
        this.CurrentValue = value;
    }

    public void Completed()
    {
        this.WriteStatus(this.MaxValue, true);
    }

    public void Dispose()
    {
        if (!this.disposed)
        {
            Console.WriteLine();

            if (OperatingSystem.IsWindows() && this.wasCursorVisible)
                Console.CursorVisible = true;

            this.disposed = true;
        }
    }

    private void WriteStatus(TValue value, bool force = false)
    {
        if (force || value != this.CurrentValue)
        {
            int oldPercent = this.ComputePercent(this.CurrentValue);
            int newPercent = this.ComputePercent(value);

            int oldBlocks = (int)Math.Round(this.valuePerBlock * double.CreateSaturating(this.CurrentValue - this.MinValue));
            int newBlocks = (int)Math.Round(this.valuePerBlock * double.CreateSaturating(value - this.MinValue));

            if (force || oldPercent != newPercent)
            {
                Console.SetCursorPosition(this.startColumn, this.startRow);
                Console.Write($"{newPercent,3}% ");
            }

            if (force || oldBlocks != newBlocks)
            {
                Span<char> fullBar = stackalloc char[ProgressBarWidth + 2];
                fullBar[0] = '[';
                fullBar[^1] = ']';
                var bar = fullBar[1..^1];
                bar[..newBlocks].Fill('=');
                bar[newBlocks..].Fill('.');
                Console.SetCursorPosition(this.startColumn + 5, this.startRow);
                Console.Out.Write(fullBar);
            }

            if (this.getStatusMessage is not null)
            {
                this.statusMessageBuffer1.Reset();
                this.getStatusMessage(this.CurrentValue, this.statusMessageBuffer1);
                var oldMessage = this.statusMessageBuffer1.Value;

                this.statusMessageBuffer2.Reset();
                this.getStatusMessage(value, this.statusMessageBuffer2);
                var newMessage = this.statusMessageBuffer2.Value;

                if (force || !oldMessage.SequenceEqual(newMessage))
                {
                    Console.SetCursorPosition(this.startColumn + 5 + ProgressBarWidth + 3, this.startRow);
                    Console.Out.Write(newMessage);
                    if (newMessage.Length < oldMessage.Length)
                    {
                        for (int i = 0; i < oldMessage.Length - newMessage.Length; i++)
                            Console.Write(' ');
                    }
                }
            }
        }
    }

    private int ComputePercent(TValue value) => (int)Math.Round(100 * double.CreateSaturating(value - this.MinValue) / double.CreateSaturating(this.MaxValue - this.MinValue));
}

public delegate void FormatStatusMessage<TValue>(TValue value, MessageWriter writer);
