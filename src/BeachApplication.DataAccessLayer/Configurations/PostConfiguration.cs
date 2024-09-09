using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class PostConfiguration : BaseEntityConfiguration<Post>
{
    public override void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.Property(p => p.Title).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Content).HasColumnType("NVARCHAR(MAX)").IsRequired();
        builder.Property(p => p.IsPublished).HasDefaultValueSql("(1)");

        builder.HasIndex(p => p.Title)
            .HasDatabaseName("IX_Title")
            .IsUnique();

        builder.ToTable("Posts");
        base.Configure(builder);
    }
}