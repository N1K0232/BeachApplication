using BeachApplication.DataAccessLayer.Entities.Common;

namespace BeachApplication.DataAccessLayer.Entities;

public class Umbrella : BaseEntity
{
    public string Letter { get; set; }

    public int Number { get; set; }

    public bool IsBusy { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; }
}