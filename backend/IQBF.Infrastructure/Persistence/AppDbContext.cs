using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // =====================================================
    // MAESTROS
    // =====================================================

    public DbSet<User> Users => Set<User>();

    public DbSet<Ship> Ships => Set<Ship>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<BL> BLs => Set<BL>();

    // =====================================================
    // OPERACIONES
    // =====================================================

    public DbSet<Shift> Shifts => Set<Shift>();

    public DbSet<Reception> Receptions => Set<Reception>();

    public DbSet<ReceptionItem> ReceptionItems => Set<ReceptionItem>();

    public DbSet<ReceptionPhoto> ReceptionPhotos => Set<ReceptionPhoto>();

    public DbSet<Dispatch> Dispatches => Set<Dispatch>();

    public DbSet<DispatchItem> DispatchItems => Set<DispatchItem>();

    public DbSet<DispatchPhoto> DispatchPhotos => Set<DispatchPhoto>();

    // =====================================================
    // ENTITY CONFIGURATION
    // =====================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS
        modelBuilder.Entity<User>()
            .HasIndex(x => x.UID)
            .IsUnique();

        // SHIPS
        modelBuilder.Entity<Ship>()
            .HasIndex(x => x.Name);

        // PRODUCTS
        modelBuilder.Entity<Product>()
            .HasIndex(x => x.Name);

        // BLS
        modelBuilder.Entity<BL>()
            .HasIndex(x => x.Code)
            .IsUnique();

        // RECEPTION ITEMS
        modelBuilder.Entity<ReceptionItem>()
            .HasOne(x => x.Reception)
            .WithMany(x => x.ReceptionItems)
            .HasForeignKey(x => x.ReceptionId);

        modelBuilder.Entity<ReceptionItem>()
            .HasOne(x => x.BL)
            .WithMany(x => x.ReceptionItems)
            .HasForeignKey(x => x.BLId);

        // RECEPTION PHOTOS
        modelBuilder.Entity<ReceptionPhoto>()
            .HasOne(x => x.Reception)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.ReceptionId);

        // DISPATCH ITEMS
        modelBuilder.Entity<DispatchItem>()
            .HasOne(x => x.Dispatch)
            .WithMany(x => x.DispatchItems)
            .HasForeignKey(x => x.DispatchId);

        modelBuilder.Entity<DispatchItem>()
            .HasOne(x => x.BL)
            .WithMany(x => x.DispatchItems)
            .HasForeignKey(x => x.BLId);

        // DISPATCH PHOTOS
        modelBuilder.Entity<DispatchPhoto>()
            .HasOne(x => x.Dispatch)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.DispatchId);
    }
}
