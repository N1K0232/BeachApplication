using Microsoft.AspNetCore.Identity;

namespace BeachApplication.Authentication.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public byte[] ProfilePhoto { get; set; }

    public string ContentType { get; set; }

    public bool IsPersistent { get; set; }

    public Guid? TenantId { get; set; }

    public Tenant Tenant { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}