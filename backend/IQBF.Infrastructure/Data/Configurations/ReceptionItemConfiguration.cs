using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class ReceptionItemConfiguration : IEntityTypeConfiguration<ReceptionItem>
{
    public void Configure(EntityTypeBuilder<ReceptionItem> builder)
    {
        builder.ToTable("ReceptionItems", table =>
        {
            table.HasCheckConstraint("CK_ReceptionItems_Quantity_Positive", "[Quantity] > 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, 3).IsRequired();
        builder.HasIndex(x => new { x.ReceptionId, x.BLId }).IsUnique();

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasOne(x => x.Reception)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ReceptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BL)
            .WithMany(x => x.ReceptionItems)
            .HasForeignKey(x => x.BLId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
