using BeachApplication.Shared.Enums;
using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Order : BaseObject
{
    public string User { get; set; } = null!;

    public OrderStatus Status { get; set; }

    public IEnumerable<OrderDetail>? OrderDetails { get; set; }
}