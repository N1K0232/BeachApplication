using BeachApplication.StorageProviders.Caching;

namespace BeachApplication.StorageProviders;

public class StorageProvider : IStorageProvider
{
    private readonly IStorageClient client;
    private readonly IStorageCache cache;

    private CancellationTokenSource cancellationTokenSource;
    private bool disposed;

    public StorageProvider(IStorageClient client, IStorageCache cache)
    {
        this.client = client;
        this.cache = cache;

        cancellationTokenSource = new CancellationTokenSource();
        disposed = false;
    }

    public async Task DeleteAsync(string path)
    {
        ThrowIfDisposed();

        await client.DeleteAsync(path, cancellationTokenSource.Token);
        await cache.DeleteAsync(path, cancellationTokenSource.Token);
    }

    public async Task<Stream?> ReadAsStreamAsync(string path)
    {
        ThrowIfDisposed();

        var cachedStream = await cache.ReadAsync(path, cancellationTokenSource.Token);
        if (cachedStream is not null)
        {
            return cachedStream;
        }

        var stream = await client.ReadAsStreamAsync(path, cancellationTokenSource.Token);
        return stream;
    }

    public async Task SaveAsync(string path, Stream stream, bool overwrite = false)
    {
        ThrowIfDisposed();

        if (overwrite)
        {
            var exists = await client.ExistsAsync(path, cancellationTokenSource.Token);
            if (exists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        await client.SaveAsync(path, stream, cancellationTokenSource.Token);
        await cache.SetAsync(path, stream, cancellationTokenSource.Token);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null!;

            disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
    }
}