using Microsoft.AspNetCore.Identity;

namespace BeachApplication.DataAccessLayer.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public virtual ICollection<Cart> Carts { get; set; }

    public virtual ICollection<Comment> Comments { get; set; }

    public virtual ICollection<Order> Orders { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}