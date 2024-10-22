using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class CartConfiguration : BaseEntityConfiguration<Cart>
{
    public override void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .IsRequired();

        builder.ToTable("Carts");
        base.Configure(builder);
    }
}