using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Inedo.ProGet.AssetDirectories;

/// <summary>
/// Client for working with a ProGet asset directory.
/// </summary>
public sealed partial class AssetDirectoryClient
{
    private static readonly char[] DirSeparators = ['/', '\\'];
    private readonly HttpClient httpClient;

    internal AssetDirectoryClient(HttpClient httpClient, string name)
    {
        this.httpClient = httpClient;
        this.AssetDirectoryName = name;
    }

    /// <summary>
    /// Gets the asset directory name.
    /// </summary>
    public string AssetDirectoryName { get; }

    /// <summary>
    /// Returns the contents of the specified asset folder.
    /// </summary>
    /// <param name="path">Full path to the asset. May be null or empty for the asset root.</param>
    /// <param name="recursive">When true, contents of all subfolders are also recursively returned.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operation.</param>
    /// <returns>Contents of the specified asset folder.</returns>
    public async IAsyncEnumerable<AssetDirectoryItem> ListContentsAsync(string? path = null, bool recursive = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"dir/{EscapePath(path)}?recursive={(recursive ? "true" : "false")})"));
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await this.httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable(responseStream, ProGetApiJsonContext.Default.AssetDirectoryItem, cancellationToken).ConfigureAwait(false))
            yield return item!;
    }
    /// <summary>
    /// Returns metadata for the asset with the specified path if it exists.
    /// </summary>
    /// <param name="path">Full path to the asset.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operation.</param>
    /// <returns>Metadata for the specified asset if it was found; otherwise null.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
    public async Task<AssetDirectoryItem?> GetItemMetadataAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"metadata/{EscapePath(path)}"));
        request.Headers.Accept.ParseAdd("application/json");

        using var response = await this.httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync(responseStream, ProGetApiJsonContext.Default.AssetDirectoryItem, cancellationToken).ConfigureAwait(false)!;
    }
    /// <summary>
    /// Updates the metadata of the specified asset.
    /// </summary>
    /// <param name="path">Full path to the asset.</param>
    /// <param name="contentType">New Content-Type of the asset if specified. When null, it will not be updated.</param>
    /// <param name="userMetadata">New user-defined metadata entries for the asset if specified. When null, it will not be updated.</param>
    /// <param name="userMetadataUpdateMode">Specifies how the <paramref name="userMetadata"/> parameter is interpreted.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operation.</param>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
    public async Task UpdateItemMetadataAsync(string path, string? contentType = null, IReadOnlyDictionary<string, AssetUserMetadata>? userMetadata = null, UserMetadataUpdateMode userMetadataUpdateMode = UserMetadataUpdateMode.CreateOrUpdate, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        if (contentType is null && userMetadata is null)
            return;

        var updateData = new AssetItemMetadataUpdate
        {
            Type = contentType,
            UserMetadataUpdateMode = userMetadataUpdateMode == UserMetadataUpdateMode.ReplaceAll ? "replace" : "update",
            UserMetadata = userMetadata
        };

        using var response = await this.httpClient.PostAsJsonAsync(BuildUrl($"metadata/{EscapePath(path)}"), updateData, ProGetApiJsonContext.Default.AssetItemMetadataUpdate, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates the specified subdirectory if it does not already exist.
    /// </summary>
    /// <param name="path">Full path of the directory to create.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operation.</param>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
    /// <remarks>
    /// It is not an error to create a directory that already exists.
    /// </remarks>
    public async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        using var response = await this.httpClient.PostAsync(BuildUrl($"dir/{EscapePath(path)}"), null, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    /// <summary>
    /// Deletes an asset item or folder.
    /// </summary>
    /// <param name="path">Full path of the asset to delete.</param>
    /// <param name="recursive">When the path refers to a directory, recursively delete contents if <c>true</c>.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operation.</param>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
    public async Task DeleteItemAsync(string path, bool recursive = false, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        using var response = await this.httpClient.PostAsync(BuildUrl($"delete/{EscapePath(path)}?recursive={(recursive ? "true" : "false")}"), null, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Opens the specified asset as a random access stream.
    /// </summary>
    /// <param name="path">Full path of the asset to download.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operation.</param>
    /// <returns><see cref="Stream"/> of the downloaded asset.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
    /// <remarks>
    /// The returned stream is not buffered in any way. It is recommended to either read in large blocks
    /// or to add a buffering layer on top. To read a file sequentially, use <see cref="DownloadFileAsync(string, CancellationToken)"/> instead.
    /// </remarks>
    public async Task<Stream> OpenRandomAccessFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var metadata = await this.GetItemMetadataAsync(path, cancellationToken).ConfigureAwait(false)
            ?? throw new ProGetApiException(HttpStatusCode.NotFound, "Asset not found.");

        if (metadata.Directory)
            throw new InvalidOperationException("Cannot open remote directory as a file.");

        return new RandomAccessDownloadStream(BuildUrl($"content/{EscapePath(path)}"), this.httpClient, metadata.Size.GetValueOrDefault());
    }
    /// <summary>
    /// Begins a download of the specified asset.
    /// </summary>
    /// <param name="path">Full path of the asset to download.</param>
    /// <param name="cancellationToken">Token used to cancel asynchronous operation.</param>
    /// <returns><see cref="Stream"/> of the downloaded asset.</returns>
    /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
    public async Task<ProGetDownloadStream> DownloadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var response = await this.httpClient.GetAsync(this.BuildUrl($"content/{EscapePath(path)}"), HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        try
        {
            await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
            return new ProGetDownloadStream(response, await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false));
        }
        catch
        {
            response.Dispose();
            throw;
        }
    }

    public async Task UploadFileAsync(string path, Stream content, string? contentType = null, Action<long>? reportProgress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(content);

        using var streamContent = getContent();
        if (!string.IsNullOrEmpty(contentType))
            streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

        using var response = await this.httpClient.PostAsync(this.BuildUrl($"content/{EscapePath(path)}"), streamContent, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);

        StreamContent getContent()
        {
            if (reportProgress is null)
                return new StreamContent(content);
            else
                return new StreamContent(new ProGetUploadStream(content, reportProgress));
        }
    }
    public async Task UploadMultipartFileAsync(string path, Stream content, long? totalSize = null, string? contentType = null, int partSize = 5 * 1024 * 1024, Action<long>? reportProgress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentOutOfRangeException.ThrowIfNegative(totalSize.GetValueOrDefault(), nameof(totalSize));
        ArgumentOutOfRangeException.ThrowIfLessThan(partSize, 1024);

        if (totalSize is null && !content.CanSeek)
            throw new ArgumentException("Total size must be specified if content stream is not seekable.");

        long totalLength = totalSize ?? (content.Length - content.Position);

        // don't bother with multipart upload if there would only be one part
        if (totalLength <= partSize)
        {
            await this.UploadFileAsync(path, content, contentType, reportProgress, cancellationToken).ConfigureAwait(false);
            return;
        }

        var srcStream = reportProgress is null ? content : new ProGetUploadStream(content, reportProgress);

        var id = Guid.NewGuid().ToString("N");
        int totalParts = (int)(totalLength / partSize);
        int finalPartSize = partSize;
        if ((totalLength % partSize) != 0)
        {
            totalParts++;
            finalPartSize = (int)(totalLength % partSize);
        }

        var baseUrl = this.BuildUrl($"content/{EscapePath(path)}?id={id}");

        for (int i = 0; i < totalParts - 1; i++)
            await uploadPart(i, partSize).ConfigureAwait(false);

        await uploadPart(totalParts - 1, finalPartSize).ConfigureAwait(false);

        using var completeResponse = await this.httpClient.PostAsync($"{baseUrl}&multipart=complete", null, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(completeResponse, cancellationToken).ConfigureAwait(false);

        async Task uploadPart(int index, int size)
        {
            using var streamContent = new PartStreamContent(new PartStream(srcStream, size));
            using var response = await this.httpClient.PostAsync($"{baseUrl}&multipart=upload&index={index}&offset={index * partSize}&totalSize={totalLength}&partSize={size}&totalParts={totalParts}", streamContent, cancellationToken).ConfigureAwait(false);
            await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task ImportArchiveAsync(Stream archive, ArchiveFormat format, string? path, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(archive);

        var escapedPath = string.Join(
            '/',
            (path ?? string.Empty)
                .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString)
        );

        var f = format switch
        {
            ArchiveFormat.Zip => "zip",
            ArchiveFormat.TarGzip => "tgz",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        var url = this.BuildUrl($"import/{path}?format={f}&overwrite={(overwrite ? "true" : "false")}");
        using var content = new StreamContent(archive);
        using var response = await this.httpClient.PostAsync(url, content, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
    }
    public async Task ExportFolderAsync(Stream output, ArchiveFormat format, string? path, bool recursive = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(output);

        var escapedPath = string.Join(
            '/',
            (path ?? string.Empty)
                .Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString)
        );

        var f = format switch
        {
            ArchiveFormat.Zip => "zip",
            ArchiveFormat.TarGzip => "tgz",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        var url = this.BuildUrl($"export/{path}?format={f}&recursive={(recursive ? "true" : "false")}");
        using var response = await this.httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        await ProGetClient.CheckResponseAsync(response, cancellationToken).ConfigureAwait(false);
        using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await responseStream.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
    }

    private static string EscapePath(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        else
            return string.Join('/', path.Split(DirSeparators, StringSplitOptions.RemoveEmptyEntries).Select(Uri.EscapeDataString));
    }
    private string BuildUrl(string url) => $"endpoints/{Uri.EscapeDataString(this.AssetDirectoryName)}/{url}";

    private sealed class PartStream(Stream stream, int partSize) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => partSize;
        public override long Position { get; set; }

        public override int Read(Span<byte> buffer)
        {
            if (buffer.IsEmpty)
                return 0;

            if (this.Position >= this.Length)
                return 0;

            int bytesToRead = (int)Math.Min(buffer.Length, this.Length - this.Position);

            int bytesRead = stream.Read(buffer[..bytesToRead]);
            this.Position += bytesRead;
            return bytesRead;
        }
        public override int Read(byte[] buffer, int offset, int count) => this.Read(buffer.AsSpan(offset, count));
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty)
                return 0;

            if (this.Position >= this.Length)
                return 0;

            int bytesToRead = (int)Math.Min(buffer.Length, this.Length - this.Position);

            int bytesRead = await stream.ReadAsync(buffer[..bytesToRead], cancellationToken).ConfigureAwait(false);
            this.Position += bytesRead;
            return bytesRead;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        public override int ReadByte()
        {
            if (this.Position < this.Length)
            {
                int res = stream.ReadByte();
                if (res >= 0)
                    this.Position++;

                return res;
            }
            else
            {
                return -1;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
        }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class PartStreamContent(PartStream partStream) : StreamContent(partStream)
    {
        protected override bool TryComputeLength(out long length)
        {
            length = partStream.Length;
            return true;
        }
    }
}
