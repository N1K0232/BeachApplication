using BeachApplication.Shared.Models.Common;

namespace BeachApplication.Shared.Models;

public class Reservation : BaseObject
{
    public DateOnly StartOn { get; set; }

    public TimeOnly StartAt { get; set; }

    public DateOnly EndsOn { get; set; }

    public TimeOnly EndsAt { get; set; }

    public string? Notes { get; set; }

    public decimal? TotalPrice { get; set; }

    public User User { get; set; } = null!;

    public Umbrella Umbrella { get; set; } = null!;
}