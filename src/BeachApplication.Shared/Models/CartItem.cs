using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class CartItem : BaseObject
{
    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public Product Product { get; set; } = null!;
}