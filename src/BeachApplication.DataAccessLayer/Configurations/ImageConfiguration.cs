using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class ImageConfiguration : BaseEntityConfiguration<Image>
{
    public override void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.Property(i => i.Path).HasMaxLength(512).IsRequired();
        builder.Property(i => i.ContentType).HasMaxLength(50).IsRequired();
        builder.Property(i => i.Description).HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.HasIndex(i => i.Path)
            .HasDatabaseName("IX_Path")
            .IsUnique();

        builder.ToTable("Images");
        base.Configure(builder);
    }
}