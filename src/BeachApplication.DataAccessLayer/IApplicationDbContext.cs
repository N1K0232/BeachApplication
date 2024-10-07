using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer;

public interface IApplicationDbContext
{
    Task DeleteAsync<T>(T entity) where T : BaseEntity;

    Task DeleteAsync<T>(IEnumerable<T> entities) where T : BaseEntity;

    ValueTask<T> GetAsync<T>(Guid id) where T : BaseEntity;

    IQueryable<T> GetData<T>(bool ignoreQueryFilters = false, bool trackingChanges = false) where T : BaseEntity;

    Task InsertAsync<T>(T entity) where T : BaseEntity;

    Task SaveAsync();

    Task ExecuteTransactionAsync(Func<Task> action);
}