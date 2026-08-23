using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShiftDate).HasColumnType("date").IsRequired();
        builder.Property(x => x.ShiftType).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.StartedAt).IsRequired();

        builder.HasIndex(x => new { x.ShipId, x.ShiftDate, x.ShiftType }).IsUnique();

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasOne(x => x.Ship)
            .WithMany(x => x.Shifts)
            .HasForeignKey(x => x.ShipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
