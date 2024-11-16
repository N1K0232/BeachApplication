using Microsoft.AspNetCore.Identity;

namespace BeachApplication.Authentication.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string ProfilePhotoPath { get; set; }

    public bool IsPersistent { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
}