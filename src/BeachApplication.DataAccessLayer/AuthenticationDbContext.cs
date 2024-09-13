using BeachApplication.DataAccessLayer.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.DataAccessLayer;

public class AuthenticationDbContext(DbContextOptions options)
        : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole,
        IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(user => user.FirstName).HasMaxLength(256).IsRequired();
            b.Property(user => user.LastName).HasMaxLength(256).IsRequired(false);
        });

        builder.Entity<ApplicationUserRole>(b =>
        {
            b.HasKey(userRole => new { userRole.UserId, userRole.RoleId });

            b.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .IsRequired();

            b.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .IsRequired();
        });
    }
}