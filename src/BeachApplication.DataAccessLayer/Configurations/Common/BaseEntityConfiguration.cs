using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations.Common;

internal abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");

        builder.Property(x => x.SecurityStamp).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasColumnType("NVARCHAR(MAX)").IsRequired().IsConcurrencyToken();

        builder.Property(x => x.CreationDate).ValueGeneratedOnAdd().HasDefaultValueSql("getutcdate()");
        builder.Property(x => x.LastModificationDate).IsRequired().ValueGeneratedOnUpdate();
    }
}