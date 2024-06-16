using Microsoft.Extensions.Caching.Memory;

namespace BeachApplication.StorageProviders.Caching;

public class StorageCache(IMemoryCache cache) : IStorageCache
{
    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        cache.Remove(path);
        return Task.CompletedTask;
    }

    public Task<Stream?> ReadAsync(string path, CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue<Stream>(path, out var stream))
        {
            return Task.FromResult(stream);
        }

        return Task.FromResult<Stream?>(null);
    }

    public Task SetAsync(string path, Stream stream, CancellationToken cancellationToken = default)
    {
        cache.Set(path, stream);
        return Task.CompletedTask;
    }
}