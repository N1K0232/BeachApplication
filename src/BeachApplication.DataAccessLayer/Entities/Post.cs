using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Post : TenantEntity
{
    public string Title { get; set; }

    public string Content { get; set; }

    public bool IsPublished { get; set; }
}