using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class ReceptionPhotoConfiguration : IEntityTypeConfiguration<ReceptionPhoto>
{
    public void Configure(EntityTypeBuilder<ReceptionPhoto> builder)
    {
        builder.ToTable("ReceptionPhotos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhotoUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255);
        builder.Property(x => x.ContentType).HasMaxLength(100);

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasOne(x => x.Reception)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.ReceptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
