using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Product : BaseObject
{
    public string? Category { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? Quantity { get; set; }

    public decimal Price { get; set; }
}