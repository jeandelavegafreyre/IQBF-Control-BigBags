using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class DispatchConfiguration : IEntityTypeConfiguration<Dispatch>
{
    public void Configure(EntityTypeBuilder<Dispatch> builder)
    {
        builder.ToTable("Dispatches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionNumber)
            .IsRequired();

        builder.Property(x => x.Plate)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(100);

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(50);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(50);

        builder.HasIndex(x => new
        {
            x.ShiftId,
            x.TransactionNumber
        })
        .IsUnique();

        builder.HasOne(x => x.Shift)
            .WithMany(x => x.Dispatches)
            .HasForeignKey(x => x.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Dispatch)
            .HasForeignKey(x => x.DispatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Photos)
            .WithOne(x => x.Dispatch)
            .HasForeignKey(x => x.DispatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
