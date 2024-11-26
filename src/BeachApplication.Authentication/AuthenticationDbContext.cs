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
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(user => user.FirstName).HasMaxLength(256).IsRequired();
            b.Property(user => user.LastName).HasMaxLength(256).IsRequired(false);

            b.Property(user => user.DateOfBirth).IsRequired(false);
            b.Property(user => user.ProfilePhoto).HasColumnType("VARBINARY(MAX)").IsRequired(false);

            b.HasOne(user => user.Tenant)
                .WithMany(tenant => tenant.Users)
                .HasForeignKey(user => user.TenantId)
                .IsRequired(false);
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

        builder.Entity<Tenant>(b =>
        {
            b.ToTable("Tenants");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).HasDefaultValueSql("newid()");

            b.Property(t => t.Name).HasMaxLength(256).IsRequired();
            b.Property(t => t.SqlConnectionString).HasMaxLength(4000).IsRequired().IsUnicode(false);

            b.Property(t => t.AzureStorageConnectionString).HasMaxLength(4000).IsRequired(false).IsUnicode(false);
            b.Property(t => t.ContainerName).HasMaxLength(256).IsRequired(false).IsUnicode(false);
        });
    }
}