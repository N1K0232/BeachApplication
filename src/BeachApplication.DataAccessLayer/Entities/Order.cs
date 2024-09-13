using BeachApplication.DataAccessLayer.Entities.Common;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.Shared.Enums;

namespace BeachApplication.DataAccessLayer.Entities;

public class Order : DeletableEntity
{
    public Guid UserId { get; set; }

    public OrderStatus Status { get; set; }

    public DateOnly OrderDate { get; set; }

    public TimeOnly OrderTime { get; set; }

    public virtual ApplicationUser User { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}