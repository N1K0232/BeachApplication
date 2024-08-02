using System.Text.Json;
using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace BeachApplication.DataAccessLayer.Caching;

public class SqlClientCache(IDistributedCache cache) : ISqlClientCache
{
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var content = await cache.GetStringAsync(id.ToString(), cancellationToken);
        return !string.IsNullOrWhiteSpace(content);
    }

    public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var content = await cache.GetStringAsync(id.ToString(), cancellationToken);
        return await ConvertToEntityAsync<T>(content, cancellationToken);
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
        var content = await ConvertToJsonAsync(entity, cancellationToken);
        await cache.SetStringAsync(entity.Id.ToString(), content, cancellationToken);
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