using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class OrderDetailConfiguration : DeletableEntityConfiguration<OrderDetail>
{
    public override void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.HasOne(o => o.Order).WithMany(o => o.OrderDetails).HasForeignKey(o => o.OrderId);
        builder.HasOne(o => o.Product).WithMany(p => p.OrderDetails).HasForeignKey(o => o.ProductId);

        builder.Property(o => o.OrderId).IsRequired();
        builder.Property(o => o.ProductId).IsRequired();

        builder.Property(o => o.Quantity).IsRequired();
        builder.Property(o => o.Price).HasPrecision(8, 2).IsRequired();
        builder.Property(o => o.Annotations).HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.ToTable("OrderDetails");
        base.Configure(builder);
    }
}