using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Post : BaseEntity
{
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public bool IsPublished { get; set; }
}