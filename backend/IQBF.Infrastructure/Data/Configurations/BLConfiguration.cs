using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class BLConfiguration : IEntityTypeConfiguration<BL>
{
    public void Configure(EntityTypeBuilder<BL> builder)
    {
        builder.ToTable("BLs", table =>
        {
            table.HasCheckConstraint("CK_BLs_TotalQuantity_Positive", "[TotalQuantity] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.ShipId, x.Code }).IsUnique();

        builder.Property(x => x.TotalQuantity).HasPrecision(18, 3).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasOne(x => x.Ship)
            .WithMany(x => x.BLs)
            .HasForeignKey(x => x.ShipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.BLs)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
