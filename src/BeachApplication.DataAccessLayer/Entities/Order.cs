using BeachApplication.DataAccessLayer.Entities.Common;
using BeachApplication.Shared.Enums;

namespace BeachApplication.DataAccessLayer.Entities;

public class Order : TenantEntity
{
    public Guid UserId { get; set; }

    public OrderStatus Status { get; set; }

    public DateOnly OrderDate { get; set; }

    public TimeOnly OrderTime { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}