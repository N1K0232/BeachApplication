using System.Text.Json;
using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace BeachApplication.DataAccessLayer.Caching;

public class SqlClientCache(IDistributedCache cache) : ISqlClientCache
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var content = await cache.GetAsync(id.ToString(), cancellationToken);
        return content != null;
    }

    public async Task<T?> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        var json = await cache.GetStringAsync(id.ToString(), cancellationToken);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var entity = JsonSerializer.Deserialize<T>(json);
        return entity;
    }

    public async Task RefreshAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await cache.RefreshAsync(id.ToString(), cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await cache.RemoveAsync(id.ToString(), cancellationToken);
    }

    public async Task SetAsync<T>(T entity, TimeSpan expirationTime, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var json = JsonSerializer.Serialize(entity);
        await cache.SetStringAsync(entity.Id.ToString(), json, cancellationToken);
    }
}