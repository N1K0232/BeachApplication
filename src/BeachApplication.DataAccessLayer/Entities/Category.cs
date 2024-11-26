using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Category : TenantEntity
{
    public string Name { get; set; }

    public string Description { get; set; }

    public virtual ICollection<Product> Products { get; set; }
}