using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Category : BaseObject
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;
}