namespace BeachApplication.DataAccessLayer.Entities.Common;

public abstract class DeletableEntity : BaseEntity
{
    public virtual bool IsDeleted { get; set; }

    public virtual DateTime? DeletedDate { get; set; }
}