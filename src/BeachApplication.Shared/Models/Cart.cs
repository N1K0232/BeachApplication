using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Cart : BaseObject
{
    public User User { get; set; } = null!;

    public ICollection<CartItem>? Items { get; set; }
}