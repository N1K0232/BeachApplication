using BeachApplication.Shared.Enums;
using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Subscription : BaseObject
{
    public User User { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly FinishDate { get; set; }

    public decimal Price { get; set; }

    public SubscriptionStatus Status { get; set; }

    public string? Notes { get; set; }
}