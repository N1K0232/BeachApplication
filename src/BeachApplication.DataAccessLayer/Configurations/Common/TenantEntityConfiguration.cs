using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations.Common;

internal abstract class TenantEntityConfiguration<T> : BaseEntityConfiguration<T> where T : TenantEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(t => t.TenantId).IsRequired();
        base.Configure(builder);
    }
}