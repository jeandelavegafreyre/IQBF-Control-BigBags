using Microsoft.EntityFrameworkCore;
using IQBF.Domain.Entities;

namespace IQBF.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
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
}
