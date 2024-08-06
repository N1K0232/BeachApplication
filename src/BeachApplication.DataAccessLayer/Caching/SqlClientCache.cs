using System.Text;
using System.Text.Json;
using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace BeachApplication.DataAccessLayer.Caching;

public class SqlClientCache(IDistributedCache distributedCache, IMemoryCache memoryCache) : ISqlClientCache
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var content = await distributedCache.GetAsync(id.ToString(), cancellationToken);
        return content != null;
    }

    public async Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var content = await distributedCache.GetAsync(id.ToString(), cancellationToken);
        if (content is null)
        {
            return null;
        }

        return await ConvertToEntityAsync<T>(content, cancellationToken);
    }

    public Task<IList<T>?> GetListAsync<T>(string key, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (memoryCache.TryGetValue<IList<T>>(key, out var entities))
        {
            return Task.FromResult(entities);
        }

        return Task.FromResult<IList<T>?>(null);
    }

    public async Task RefreshAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await distributedCache.RefreshAsync(id.ToString(), cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await distributedCache.RemoveAsync(id.ToString(), cancellationToken);
    }

    public async Task SetAsync<T>(T entity, TimeSpan expirationTime, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var content = await ConvertToBytesAsync(entity, cancellationToken);
        await distributedCache.SetAsync(entity.Id.ToString(), content, cancellationToken);
    }

    public Task SetAsync<T>(string key, IList<T> entities, TimeSpan expirationTime, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(key, entities, expirationTime);
        return Task.CompletedTask;
    }

    private static Task<byte[]> ConvertToBytesAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        var content = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entity));
        return Task.FromResult(content);
    }

    private static async Task<T?> ConvertToEntityAsync<T>(byte[] content, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var stream = new MemoryStream(content);
        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
    }
}