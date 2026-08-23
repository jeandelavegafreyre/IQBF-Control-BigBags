using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class ShipConfiguration : IEntityTypeConfiguration<Ship>
{
    public void Configure(EntityTypeBuilder<Ship> builder)
    {
        builder.ToTable("Ships");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Status).HasConversion<int>().IsRequired();

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasMany(x => x.BLs)
            .WithOne(x => x.Ship)
            .HasForeignKey(x => x.ShipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Shifts)
            .WithOne(x => x.Ship)
            .HasForeignKey(x => x.ShipId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
