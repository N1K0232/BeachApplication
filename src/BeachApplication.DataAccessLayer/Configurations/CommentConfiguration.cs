using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class CommentConfiguration : BaseEntityConfiguration<Comment>
{
    public override void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.Property(c => c.Title).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Text).HasColumnType("NVARCHAR(MAX)").IsRequired();

        builder.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .IsRequired();

        builder.HasIndex(c => new { c.UserId, c.Title })
            .HasDatabaseName("IX_UserComment")
            .IsUnique();

        builder.ToTable("Comments");
        base.Configure(builder);
    }
}