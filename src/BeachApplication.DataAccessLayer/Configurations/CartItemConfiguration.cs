using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class CartItemConfiguration : BaseEntityConfiguration<CartItem>
{
    public override void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.Property(c => c.Quantity).IsRequired();
        builder.Property(c => c.Price).HasPrecision(8, 2).IsRequired();
        builder.Property(c => c.Notes).HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.HasOne(c => c.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(c => c.CartId)
            .IsRequired();

        builder.HasOne(c => c.Product)
            .WithMany(p => p.Items)
            .HasForeignKey(c => c.ProductId)
            .IsRequired();

        builder.ToTable("CartItems");
        base.Configure(builder);
    }
}