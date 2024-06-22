using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class OrderDetail : BaseObject
{
    public int? Quantity { get; set; }

    public decimal Price { get; set; }

    public IEnumerable<string>? Annotations { get; set; }
}