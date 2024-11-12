using BeachApplication.Authentication.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.Authentication;

public class AuthenticationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole,
      IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IDataProtectionKeyContext
{
    public AuthenticationDbContext()
    {
    }

    public AuthenticationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

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

        builder.Entity<DataProtectionKey>(b =>
        {
            b.ToTable("DataProtectionKeys");
            b.HasKey(k => k.Id);
            b.Property(k => k.Id);

            b.Property(k => k.FriendlyName).HasMaxLength(256).IsRequired(false);
            b.Property(k => k.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        });
    }
}