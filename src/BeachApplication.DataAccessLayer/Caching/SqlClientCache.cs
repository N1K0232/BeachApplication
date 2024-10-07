using System.Text;
using System.Text.Json;
using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BeachApplication.DataAccessLayer.Caching;

public class SqlClientCache(IDistributedCache cache, IOptions<DistributedCacheEntryOptions> cacheOptions, ILogger<SqlClientCache> logger) : ISqlClientCache
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("checking if the entity with id {id} is inside the cache table", id);
        return await ExistsInternalAsync(id, cancellationToken);
    }

    public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        logger.LogInformation("finding entity in cache");
        var content = await cache.GetAsync(id.ToString(), cancellationToken);

        if (content is null)
        {
            return null;
        }

        return await ConvertToEntityAsync<T>(content);
    }

    public async Task RefreshAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Updating cache expiration for entity {id}", id);
        await cache.RefreshAsync(id.ToString(), cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("deleting the entity with id {id} from cache", id);
        await cache.RemoveAsync(id.ToString(), cancellationToken);
    }

    public async Task SetAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        logger.LogInformation("inserting a new entity in the cache");

        var bytes = await ConvertToBytesAsync(entity);
        await cache.SetAsync(entity.Id.ToString(), bytes, cacheOptions.Value, cancellationToken);
    }

    public async Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        logger.LogInformation("updating entity with id {id}", entity.Id);

        var exists = await ExistsInternalAsync(entity.Id, cancellationToken);
        if (exists)
        {
            var bytes = await ConvertToBytesAsync(entity);
            await cache.RemoveAsync(entity.Id.ToString(), cancellationToken);
            await cache.SetAsync(entity.Id.ToString(), bytes, cacheOptions.Value, cancellationToken);
        }
    }

    private static Task<byte[]> ConvertToBytesAsync<T>(T entity) where T : BaseEntity
    {
        var json = JsonSerializer.Serialize(entity);
        var bytes = Encoding.UTF8.GetBytes(json);

        return Task.FromResult(bytes);
    }

    private static Task<T> ConvertToEntityAsync<T>(byte[] content) where T : BaseEntity
    {
        var json = Encoding.UTF8.GetString(content);
        var entity = JsonSerializer.Deserialize<T>(json);

        return Task.FromResult(entity);
    }

    private async Task<bool> ExistsInternalAsync(Guid id, CancellationToken cancellationToken)
    {
        var content = await cache.GetAsync(id.ToString(), cancellationToken);
        return content != null;
    }
}