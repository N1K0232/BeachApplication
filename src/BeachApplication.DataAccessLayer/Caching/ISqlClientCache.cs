using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Caching;

public interface ISqlClientCache
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<T> GetAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task RefreshAsync(Guid id, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);

    Task SetAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;

    Task UpdateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity
}