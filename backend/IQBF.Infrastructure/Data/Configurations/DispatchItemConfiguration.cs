using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class DispatchItemConfiguration : IEntityTypeConfiguration<DispatchItem>
{
    public void Configure(EntityTypeBuilder<DispatchItem> builder)
    {
        builder.ToTable("DispatchItems", table =>
        {
            table.HasCheckConstraint("CK_DispatchItems_Quantity_Positive", "[Quantity] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        builder.HasIndex(x => new { x.DispatchId, x.BLId }).IsUnique();

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasOne(x => x.Dispatch)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.DispatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BL)
            .WithMany(x => x.DispatchItems)
            .HasForeignKey(x => x.BLId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
