using IQBF.Domain.Common;
using IQBF.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Infrastructure.Data;

public class IQBFDbContext : DbContext
{
    public IQBFDbContext(DbContextOptions<IQBFDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ship> Ships => Set<Ship>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<BL> BLs => Set<BL>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Reception> Receptions => Set<Reception>();
    public DbSet<ReceptionItem> ReceptionItems => Set<ReceptionItem>();
    public DbSet<ReceptionPhoto> ReceptionPhotos => Set<ReceptionPhoto>();
    public DbSet<Dispatch> Dispatches => Set<Dispatch>();
    public DbSet<DispatchItem> DispatchItems => Set<DispatchItem>();
    public DbSet<DispatchPhoto> DispatchPhotos => Set<DispatchPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IQBFDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        ApplyAuditDates();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditDates();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditDates()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
