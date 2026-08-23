using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class ReceptionConfiguration : IEntityTypeConfiguration<Reception>
{
    public void Configure(EntityTypeBuilder<Reception> builder)
    {
        builder.ToTable("Receptions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TerminalTruck).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(100);

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasOne(x => x.Shift)
            .WithMany(x => x.Receptions)
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Reception)
            .HasForeignKey(x => x.ReceptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Photos)
            .WithOne(x => x.Reception)
            .HasForeignKey(x => x.ReceptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
