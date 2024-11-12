using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class CartConfiguration : BaseEntityConfiguration<Cart>
{
    public override void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.Property(c => c.UserId).IsRequired();

        builder.ToTable("Carts");
        base.Configure(builder);
    }
}