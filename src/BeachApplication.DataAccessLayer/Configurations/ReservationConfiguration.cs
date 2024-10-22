using BeachApplication.DataAccessLayer.Configurations.Common;
using BeachApplication.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations;

internal class ReservationConfiguration : DeletableEntityConfiguration<Reservation>
{
    public override void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.Property(r => r.StartOn).IsRequired();
        builder.Property(r => r.StartAt).IsRequired();

        builder.Property(r => r.EndsAt).IsRequired();
        builder.Property(r => r.EndsOn).IsRequired();

        builder.Property(r => r.Notes).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        builder.Property(r => r.TotalPrice).HasPrecision(8, 2).IsRequired(false);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserId)
            .IsRequired();

        builder.HasOne(r => r.Umbrella)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UmbrellaId)
            .IsRequired();

        builder.ToTable("Reservations");
        base.Configure(builder);
    }
}