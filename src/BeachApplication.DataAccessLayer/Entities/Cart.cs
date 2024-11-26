using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Cart : TenantEntity
{
    public Guid UserId { get; set; }

    public virtual ICollection<CartItem> Items { get; set; }
}