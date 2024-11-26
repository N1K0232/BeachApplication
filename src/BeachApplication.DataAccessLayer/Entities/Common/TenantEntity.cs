namespace BeachApplication.DataAccessLayer.Entities.Common;

public abstract class TenantEntity : BaseEntity
{
    public Guid TenantId { get; set; }
}