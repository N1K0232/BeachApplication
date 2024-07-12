
using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Internal;

public interface IEntityStore
{
    Task GenerateConcurrencyStampAsync<T>(T entity) where T : BaseEntity;

    Task GenerateSecurityStampAsync<T>(T entity) where T : BaseEntity;
}