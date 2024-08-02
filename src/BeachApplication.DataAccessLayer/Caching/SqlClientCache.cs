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

        var content = await distributedCache.GetStringAsync(id.ToString(), cancellationToken);
        return !string.IsNullOrWhiteSpace(content);
    }

    public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var content = await distributedCache.GetStringAsync(id.ToString(), cancellationToken);
        return await ConvertToEntityAsync<T>(content, cancellationToken);
    }

    public Task<IList<T>> GetListAsync<T>(string key, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (memoryCache.TryGetValue<IList<T>>(key, out var entities))
        {
            return Task.FromResult(entities);
        }

        return Task.FromResult<IList<T>>(null);
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
        var content = await ConvertToJsonAsync(entity, cancellationToken);
        await distributedCache.SetStringAsync(entity.Id.ToString(), content, cancellationToken);
    }

    public Task SetAsync<T>(string key, IList<T> entities, TimeSpan expirationTime, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        memoryCache.Set(key, entities, expirationTime);
        return Task.CompletedTask;
    }

    private static Task<string> ConvertToJsonAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        var content = JsonSerializer.Serialize(entity);
        return Task.FromResult(content);
    }

    private static Task<T> ConvertToEntityAsync<T>(string content, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        var entity = JsonSerializer.Deserialize<T>(content);
        return Task.FromResult(entity);
    }
}