using BeachApplication.DataAccessLayer.Entities.Common;
using BeachApplication.DataAccessLayer.Entities.Identity;

namespace BeachApplication.DataAccessLayer.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }

    public virtual ApplicationUser User { get; set; }

    public virtual ICollection<CartItem> Items { get; set; }
}