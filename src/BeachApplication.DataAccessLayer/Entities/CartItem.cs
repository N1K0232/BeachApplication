using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string Notes { get; set; }

    public virtual Cart Cart { get; set; }

    public virtual Product Product { get; set; }
}