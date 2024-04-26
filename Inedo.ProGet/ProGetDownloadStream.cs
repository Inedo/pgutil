namespace Inedo.ProGet;

public sealed class ProGetDownloadStream : Stream
{
    private readonly HttpResponseMessage response;
    private readonly Stream contentStream;

    internal ProGetDownloadStream(HttpResponseMessage response, Stream stream)
    {
        this.response = response;
        this.contentStream = stream;
    }

    public event EventHandler? BytesReadChanged;

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
    public long BytesRead { get; private set; }
    public string? ContentType => this.response.Content.Headers.ContentType?.ToString();
    public long? ContentLength => this.response.Content.Headers.ContentLength;
    public string? FileName => this.response.Content.Headers.ContentDisposition?.FileName?.Trim('\"', '\'');

    public override int Read(Span<byte> buffer)
    {
        int res = this.contentStream.Read(buffer);
        this.IncrementBytesRead(res);
        return res;
    }
    public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
    public override int ReadByte()
    {
        int res = this.contentStream.ReadByte();
        if (res >= 0)
            this.IncrementBytesRead(1);
        return res;
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        int res = await this.contentStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        this.IncrementBytesRead(res);
        return res;
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

    public override void Flush()
    {
    }
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    private void IncrementBytesRead(int count)
    {
        if (count > 0)
        {
            this.BytesRead += count;
            this.BytesReadChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
