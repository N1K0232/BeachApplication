using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.DataProtectionLayer;

public class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DataProtectionKey>(builder =>
        {
            builder.ToTable("DataProtectionKeys");
            builder.HasKey(k => k.Id);
            builder.Property(k => k.Id).UseIdentityColumn(1, 1);

            builder.Property(k => k.FriendlyName).HasMaxLength(100).IsRequired(false);
            builder.Property(k => k.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        });

        base.OnModelCreating(modelBuilder);
    }
}