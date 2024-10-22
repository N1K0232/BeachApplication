using BeachApplication.DataAccessLayer.Entities.Common;
using BeachApplication.DataAccessLayer.Entities.Identity;

namespace BeachApplication.DataAccessLayer.Entities;

public class Reservation : DeletableEntity
{
    public Guid UserId { get; set; }

    public Guid UmbrellaId { get; set; }

    public DateOnly StartOn { get; set; }

    public TimeOnly StartAt { get; set; }

    public DateOnly EndsOn { get; set; }

    public TimeOnly EndsAt { get; set; }

    public string Notes { get; set; }

    public decimal? TotalPrice { get; set; }

    public virtual ApplicationUser User { get; set; }

    public virtual Umbrella Umbrella { get; set; }
}