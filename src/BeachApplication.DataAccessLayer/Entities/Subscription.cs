using BeachApplication.DataAccessLayer.Entities.Common;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.Shared.Enums;

namespace BeachApplication.DataAccessLayer.Entities;

public class Subscription : DeletableEntity
{
    public Guid UserId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly FinishDate { get; set; }

    public decimal Price { get; set; }

    public SubscriptionStatus Status { get; set; }

    public string Notes { get; set; }

    public virtual ApplicationUser User { get; set; }
}