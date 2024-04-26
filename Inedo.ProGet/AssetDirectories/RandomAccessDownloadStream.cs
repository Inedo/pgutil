using System.Net.Http.Headers;

namespace Inedo.ProGet.AssetDirectories;

public partial class AssetDirectoryClient
{
    private sealed class RandomAccessDownloadStream(string contentUrl, HttpClient httpClient, long length) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => length;
        public override long Position { get; set; }

        public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        public override int Read(Span<byte> buffer)
        {
            if (buffer.IsEmpty)
                return 0;

            int bytesToRead = (int)Math.Min(buffer.Length, this.Length - this.Position);

            using var request = new HttpRequestMessage(HttpMethod.Get, contentUrl);
            request.Headers.Range = new RangeHeaderValue(this.Position, this.Position + bytesToRead);

            using var response = httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead);
            ProGetClient.CheckResponse(response);

            using var responseStream = response.Content.ReadAsStream();
            responseStream.ReadExactly(buffer[..bytesToRead]);
            return bytesToRead;
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty)
                return 0;

            int bytesToRead = (int)Math.Min(buffer.Length, this.Length - this.Position);

            using var request = new HttpRequestMessage(HttpMethod.Get, contentUrl);
            request.Headers.Range = new RangeHeaderValue(this.Position, this.Position + bytesToRead);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

            using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await responseStream.ReadExactlyAsync(buffer[..bytesToRead], cancellationToken).ConfigureAwait(false);
            return bytesToRead;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => this.Position + offset,
                SeekOrigin.End => this.Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };
        }
        public override void Flush()
        {
        }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}