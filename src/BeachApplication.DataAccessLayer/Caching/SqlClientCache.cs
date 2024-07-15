using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.Extensions.Caching.Memory;

namespace BeachApplication.DataAccessLayer.Caching;

public class SqlClientCache(IMemoryCache cache) : ISqlClientCache
{
    public Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        if (cache.TryGetValue<T>(id, out var entity))
        {
            return Task.FromResult(entity);
        }

        return Task.FromResult<T>(null);
    }

    public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cache.Remove(id);
        return Task.CompletedTask;
    }

    public Task SetAsync<T>(T entity, TimeSpan expirationTime, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        cache.Set(entity.Id, entity, expirationTime);
        return Task.CompletedTask;
    }
}