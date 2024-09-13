using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class SubscriptionConfiguration : DeletableEntityConfiguration<Subscription>
{
    public override void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(s => s.StartDate).IsRequired();
        builder.Property(s => s.FinishDate).IsRequired();

        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(s => s.Notes).HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .IsRequired();

        builder.ToTable("Subscriptions");
        base.Configure(builder);
    }
}