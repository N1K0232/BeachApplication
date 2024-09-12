using System.Text;
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

    public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cancellationToken.ThrowIfCancellationRequested();

        var bytes = await cache.GetAsync(id.ToString(), cancellationToken);
        if (bytes is null)
        {
            return null;
        }

        var json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<T>(json);
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
        var bytes = Encoding.UTF8.GetBytes(json);

        await cache.SetAsync(entity.Id.ToString(), bytes, cancellationToken);
    }
}