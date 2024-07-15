using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Caching;

public interface ISqlClientCache
{
    Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task SetAsync<T>(T entity, TimeSpan expirationTime, CancellationToken cancellationToken = default) where T : BaseEntity;
}