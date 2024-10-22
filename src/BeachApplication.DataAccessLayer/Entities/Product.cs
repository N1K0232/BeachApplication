using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Product : DeletableEntity
{
    public Guid CategoryId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int? Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Category Category { get; set; }

    public virtual ICollection<CartItem> Items { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}