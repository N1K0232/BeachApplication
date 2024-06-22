using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class OrderConfiguration : DeletableEntityConfiguration<Order>
{
    public override void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(o => o.OrderDate).IsRequired();
        builder.Property(o => o.OrderTime).IsRequired();

        builder.ToTable("Orders");
        base.Configure(builder);
    }
}