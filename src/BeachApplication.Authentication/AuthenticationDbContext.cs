using BeachApplication.Authentication.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.Authentication;

public class AuthenticationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public AuthenticationDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(user => user.FirstName).HasMaxLength(256).IsRequired();
            b.Property(user => user.LastName).HasMaxLength(256).IsRequired(false);

            b.Property(user => user.RefreshToken).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
            b.Property(user => user.RefreshTokenExpirationDate).IsRequired(false);
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