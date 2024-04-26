namespace Inedo.ProGet;

internal sealed class ProGetUploadStream(Stream stream, Action<long> reportProgress) : Stream
{
    private long totalRead;

    public override bool CanRead => true;
    public override bool CanSeek => stream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => stream.Length;
    public override long Position { get => stream.Position; set => stream.Position = value; }

    public override int Read(Span<byte> buffer)
    {
        int count = stream.Read(buffer);
        this.Report(count);
        return count;
    }
    public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
    public override int ReadByte()
    {
        int res = stream.ReadByte();
        this.Report(res >= 0 ? 1 : 0);
        return res;
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int count = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        this.Report(count);
        return count;
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    public override void Flush() => stream.Flush();
    public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    private void Report(int read)
    {
        if (read > 0)
        {
            this.totalRead += read;
            reportProgress(this.totalRead);
        }
    }
}
