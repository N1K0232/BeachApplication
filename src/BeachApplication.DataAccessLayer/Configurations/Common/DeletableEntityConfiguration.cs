using BeachApplication.DataAccessLayer.Entities.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BeachApplication.DataAccessLayer.Configurations.Common;

internal abstract class DeletableEntityConfiguration<T> : BaseEntityConfiguration<T> where T : DeletableEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.IsDeleted).ValueGeneratedOnAdd().HasDefaultValueSql("(0)");
        builder.Property(x => x.DeletedAt).IsRequired(false);

        base.Configure(builder);
    }
}