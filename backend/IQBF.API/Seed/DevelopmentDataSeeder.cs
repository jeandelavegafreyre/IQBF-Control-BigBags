using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.API.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IQBFDbContext>();

        const string createdBy = "DEV-SEED";

        var ship1 = await db.Ships.FirstOrDefaultAsync(x => x.Name == "NAVE DEMO 01");
        if (ship1 is null)
        {
            ship1 = new Ship
            {
                Name = "NAVE DEMO 01",
                Status = ShipStatus.Active,
                CreatedBy = createdBy
            };
            db.Ships.Add(ship1);
        }

        var ship2 = await db.Ships.FirstOrDefaultAsync(x => x.Name == "NAVE DEMO 02");
        if (ship2 is null)
        {
            ship2 = new Ship
            {
                Name = "NAVE DEMO 02",
                Status = ShipStatus.Active,
                CreatedBy = createdBy
            };
            db.Ships.Add(ship2);
        }

        var productA = await db.Products.FirstOrDefaultAsync(x => x.Name == "PRODUCTO IQBF A");
        if (productA is null)
        {
            productA = new Product
            {
                Name = "PRODUCTO IQBF A",
                IsActive = true,
                CreatedBy = createdBy
            };
            db.Products.Add(productA);
        }

        var productB = await db.Products.FirstOrDefaultAsync(x => x.Name == "PRODUCTO IQBF B");
        if (productB is null)
        {
            productB = new Product
            {
                Name = "PRODUCTO IQBF B",
                IsActive = true,
                CreatedBy = createdBy
            };
            db.Products.Add(productB);
        }

        await db.SaveChangesAsync();

        if (!await db.BLs.AnyAsync(x => x.Code == "BL-DEMO-001"))
        {
            db.BLs.Add(new BL
            {
                Code = "BL-DEMO-001",
                TotalQuantity = 1000,
                IsActive = true,
                ShipId = ship1.Id,
                ProductId = productA.Id,
                CreatedBy = createdBy
            });
        }

        if (!await db.BLs.AnyAsync(x => x.Code == "BL-DEMO-002"))
        {
            db.BLs.Add(new BL
            {
                Code = "BL-DEMO-002",
                TotalQuantity = 800,
                IsActive = true,
                ShipId = ship1.Id,
                ProductId = productB.Id,
                CreatedBy = createdBy
            });
        }

        if (!await db.BLs.AnyAsync(x => x.Code == "BL-DEMO-003"))
        {
            db.BLs.Add(new BL
            {
                Code = "BL-DEMO-003",
                TotalQuantity = 1200,
                IsActive = true,
                ShipId = ship2.Id,
                ProductId = productA.Id,
                CreatedBy = createdBy
            });
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasOpenShift = await db.Shifts.AnyAsync(x =>
            x.ShipId == ship1.Id &&
            x.ShiftDate == today &&
            x.Status == ShiftStatus.Open);

        if (!hasOpenShift)
        {
            db.Shifts.Add(new Shift
            {
                ShipId = ship1.Id,
                ShiftDate = today,
                ShiftType = ShiftType.Day,
                Status = ShiftStatus.Open,
                StartedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            });
        }

        await db.SaveChangesAsync();
    }
}
