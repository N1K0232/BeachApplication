using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Post : BaseObject
{
    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public bool IsPublished { get; set; }
}