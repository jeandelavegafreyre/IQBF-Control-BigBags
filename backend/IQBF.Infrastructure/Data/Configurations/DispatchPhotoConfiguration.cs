using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IQBF.Infrastructure.Data.Configurations;

public class DispatchPhotoConfiguration : IEntityTypeConfiguration<DispatchPhoto>
{
    public void Configure(EntityTypeBuilder<DispatchPhoto> builder)
    {
        builder.ToTable("DispatchPhotos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PhotoUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(255);
        builder.Property(x => x.ContentType).HasMaxLength(100);

        builder.Property(x => x.CreatedBy).HasMaxLength(50);
        builder.Property(x => x.UpdatedBy).HasMaxLength(50);

        builder.HasOne(x => x.Dispatch)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.DispatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
