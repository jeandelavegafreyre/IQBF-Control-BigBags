using IQBF.Application.DTOs.Reports;
using IQBF.Application.Interfaces;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public sealed class ManagementStatusService : IManagementStatusService
{
    private readonly IQBFDbContext _db;

    public ManagementStatusService(IQBFDbContext db)
    {
        _db = db;
    }

    public async Task<ManagementInProcessDto> GetInProcessAsync(CancellationToken cancellationToken = default)
    {
        var bls = await _db.BLs
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.ShipId,
                ShipName = x.Ship!.Name,
                x.TotalQuantity,
                ProductName = x.Product!.Name
            })
            .ToListAsync(cancellationToken);

        var receptions = await _db.Receptions
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.CreatedAt,
                ShipId = x.Shift!.ShipId
            })
            .ToListAsync(cancellationToken);

        var dispatches = await _db.Dispatches
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.CreatedAt,
                ShipId = x.Shift!.ShipId
            })
            .ToListAsync(cancellationToken);

        var receptionItems = await _db.ReceptionItems
            .AsNoTracking()
            .Select(x => new
            {
                ShipId = x.BL!.ShipId,
                x.Quantity
            })
            .ToListAsync(cancellationToken);

        var dispatchItems = await _db.DispatchItems
            .AsNoTracking()
            .Select(x => new
            {
                ShipId = x.BL!.ShipId,
                x.Quantity
            })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var shipIds = bls.Select(x => x.ShipId).Distinct().ToList();
        var rows = new List<ManagementInProcessShipDto>();

        foreach (var shipId in shipIds)
        {
            var shipBls = bls.Where(x => x.ShipId == shipId).ToList();
            var shipReceptions = receptions.Where(x => x.ShipId == shipId).ToList();
            var shipDispatches = dispatches.Where(x => x.ShipId == shipId).ToList();
            var received = receptionItems.Where(x => x.ShipId == shipId).Sum(x => x.Quantity);
            var dispatched = dispatchItems.Where(x => x.ShipId == shipId).Sum(x => x.Quantity);
            var available = received - dispatched;

            if (received <= 0 || available <= 0 || shipReceptions.Count == 0)
                continue;

            var firstReception = shipReceptions.Min(x => x.CreatedAt);
            var lastDispatch = shipDispatches.Count == 0 ? (DateTime?)null : shipDispatches.Max(x => x.CreatedAt);
            var lastMovement = new[]
            {
                shipReceptions.Max(x => x.CreatedAt),
                lastDispatch ?? DateTime.MinValue
            }.Max();

            var progress = received > 0 ? Math.Clamp((double)dispatched / received * 100, 0, 100) : 0;
            var durationHours = Math.Max(0, (now - firstReception).TotalHours);

            rows.Add(new ManagementInProcessShipDto(
                shipId,
                shipBls.First().ShipName,
                firstReception,
                lastDispatch,
                lastMovement,
                shipBls.Sum(x => x.TotalQuantity),
                received,
                dispatched,
                available,
                Math.Round(progress, 1),
                Math.Round(durationHours, 1),
                shipBls.Count,
                shipReceptions.Count,
                shipDispatches.Count,
                shipBls.Select(x => x.ProductName).Distinct().OrderBy(x => x).ToList()));
        }

        var ordered = rows
            .OrderByDescending(x => x.CalendarDurationHours)
            .ThenBy(x => x.ShipName)
            .ToList();

        return new ManagementInProcessDto(
            ordered.Count,
            ordered.Sum(x => x.ReceivedQuantity),
            ordered.Sum(x => x.DispatchedQuantity),
            ordered.Sum(x => x.AvailableQuantity),
            ordered);
    }
}
