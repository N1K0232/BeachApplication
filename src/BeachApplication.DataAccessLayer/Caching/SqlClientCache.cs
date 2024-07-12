using System.Text;
using System.Text.Json;
using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace BeachApplication.DataAccessLayer.Caching;

public class SqlClientCache(IDistributedCache cache) : ISqlClientCache
{
    private const string EntityKey = "Entity-{0}";

    public async Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var key = GenerateKey(id);
        var bytes = await cache.GetAsync(key, cancellationToken);

        return ConvertBytesToEntity<T>(bytes);
    }

    public async Task RefreshAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = GenerateKey(id);
        await cache.RefreshAsync(key, cancellationToken);
    }

    public async Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var key = GenerateKey(id);
        await cache.RemoveAsync(key, cancellationToken);
    }

    public async Task SetAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var key = GenerateKey(entity.Id);
        var bytes = ConvertEntityToBytes(entity);

        await cache.SetAsync(key, bytes, cancellationToken);
    }

    private static string GenerateKey(Guid id)
    {
        return string.Format(EntityKey, id);
    }

    private static byte[] ConvertEntityToBytes<T>(T entity) where T : BaseEntity
    {
        var jsonContent = JsonSerializer.Serialize(entity);
        return Encoding.UTF8.GetBytes(jsonContent);
    }

    private static T ConvertBytesToEntity<T>(byte[] content) where T : BaseEntity
    {
        var jsonContent = Encoding.UTF8.GetString(content);
        return JsonSerializer.Deserialize<T>(jsonContent);
    }
}