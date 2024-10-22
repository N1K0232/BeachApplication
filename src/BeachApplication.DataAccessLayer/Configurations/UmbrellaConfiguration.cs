using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class UmbrellaConfiguration : BaseEntityConfiguration<Umbrella>
{
    public override void Configure(EntityTypeBuilder<Umbrella> builder)
    {
        builder.Property(u => u.Letter).HasMaxLength(1).IsRequired();
        builder.Property(u => u.Number).IsRequired();
        builder.Property(u => u.IsBusy).HasDefaultValueSql("0").IsRequired();

        builder.ToTable("Umbrellas");
        base.Configure(builder);
    }
}