using IQBF.Application.DTOs.Dashboard;
using IQBF.Application.Interfaces;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IQBFDbContext _db;

    public DashboardService(IQBFDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // RESUMEN GENERAL POR NAVE
    // ============================================================

    public async Task<ShipSummaryDto> GetShipSummaryAsync(
        Guid shipId,
        CancellationToken cancellationToken = default)
    {
        var ship = await _db.Ships
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == shipId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Nave no encontrada.");

        var bls = await _db.BLs
            .AsNoTracking()
            .Where(x => x.ShipId == shipId && x.IsActive)
            .Include(x => x.Product)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        var blIds = bls
            .Select(x => x.Id)
            .ToArray();

        var receptions = await _db.ReceptionItems
            .AsNoTracking()
            .Where(x => blIds.Contains(x.BLId))
            .GroupBy(x => x.BLId)
            .Select(g => new
            {
                BLId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToDictionaryAsync(
                x => x.BLId,
                x => x.Quantity,
                cancellationToken);

        var dispatches = await _db.DispatchItems
            .AsNoTracking()
            .Where(x => blIds.Contains(x.BLId))
            .GroupBy(x => x.BLId)
            .Select(g => new
            {
                BLId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToDictionaryAsync(
                x => x.BLId,
                x => x.Quantity,
                cancellationToken);

        var balances = bls
            .Select(bl =>
            {
                var received = receptions.TryGetValue(
                    bl.Id,
                    out var receivedValue)
                    ? receivedValue
                    : 0m;

                var dispatched = dispatches.TryGetValue(
                    bl.Id,
                    out var dispatchedValue)
                    ? dispatchedValue
                    : 0m;

                var available =
                    received - dispatched;

                var pendingReception =
                    bl.TotalQuantity - received;

                var receptionProgress =
                    bl.TotalQuantity > 0
                        ? Math.Round(
                            received / bl.TotalQuantity * 100m,
                            2)
                        : 0m;

                var dispatchProgress =
                    bl.TotalQuantity > 0
                        ? Math.Round(
                            dispatched / bl.TotalQuantity * 100m,
                            2)
                        : 0m;

                return new BLBalanceDto(
                    bl.Id,
                    bl.Code,
                    bl.Product?.Name ?? string.Empty,
                    bl.TotalQuantity,
                    received,
                    dispatched,
                    available,
                    pendingReception,
                    receptionProgress,
                    dispatchProgress);
            })
            .ToList();

        var totalQuantity =
            balances.Sum(x => x.TotalQuantity);

        var receivedQuantity =
            balances.Sum(x => x.ReceivedQuantity);

        var dispatchedQuantity =
            balances.Sum(x => x.DispatchedQuantity);

        var availableQuantity =
            receivedQuantity - dispatchedQuantity;

        var pendingReception =
            totalQuantity - receivedQuantity;

        var receptionProgress =
            totalQuantity > 0
                ? Math.Round(
                    receivedQuantity / totalQuantity * 100m,
                    2)
                : 0m;

        var dispatchProgress =
            totalQuantity > 0
                ? Math.Round(
                    dispatchedQuantity / totalQuantity * 100m,
                    2)
                : 0m;

        return new ShipSummaryDto(
            ship.Id,
            ship.Name,
            totalQuantity,
            receivedQuantity,
            dispatchedQuantity,
            availableQuantity,
            pendingReception,
            receptionProgress,
            dispatchProgress,
            balances);
    }

    // ============================================================
    // RESUMEN POR TURNO
    // ============================================================

    public async Task<ShiftSummaryDto> GetShiftSummaryAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        var shift = await _db.Shifts
            .AsNoTracking()
            .Include(x => x.Ship)
            .FirstOrDefaultAsync(
                x => x.Id == shiftId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Turno no encontrado.");

        // --------------------------------------------------------
        // RECEPCIONES DEL TURNO AGRUPADAS POR BL
        // --------------------------------------------------------

        var receptions = await _db.ReceptionItems
            .AsNoTracking()
            .Where(x =>
                x.Reception != null &&
                x.Reception.ShiftId == shiftId)
            .GroupBy(x => x.BLId)
            .Select(g => new
            {
                BLId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToDictionaryAsync(
                x => x.BLId,
                x => x.Quantity,
                cancellationToken);

        // --------------------------------------------------------
        // DESPACHOS DEL TURNO AGRUPADOS POR BL
        // --------------------------------------------------------

        var dispatches = await _db.DispatchItems
            .AsNoTracking()
            .Where(x =>
                x.Dispatch != null &&
                x.Dispatch.ShiftId == shiftId)
            .GroupBy(x => x.BLId)
            .Select(g => new
            {
                BLId = g.Key,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToDictionaryAsync(
                x => x.BLId,
                x => x.Quantity,
                cancellationToken);

        // --------------------------------------------------------
        // OBTENER LOS BL QUE TUVIERON MOVIMIENTO EN EL TURNO
        // --------------------------------------------------------

        var blIds = receptions.Keys
            .Union(dispatches.Keys)
            .ToArray();

        var bls = await _db.BLs
            .AsNoTracking()
            .Where(x => blIds.Contains(x.Id))
            .Include(x => x.Product)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        // --------------------------------------------------------
        // RESUMEN POR BL DEL TURNO
        // --------------------------------------------------------

        var balances = bls
            .Select(bl =>
            {
                var received = receptions.TryGetValue(
                    bl.Id,
                    out var receivedValue)
                    ? receivedValue
                    : 0m;

                var dispatched = dispatches.TryGetValue(
                    bl.Id,
                    out var dispatchedValue)
                    ? dispatchedValue
                    : 0m;

                var netQuantity =
                    received - dispatched;

                return new ShiftBLBalanceDto(
                    bl.Id,
                    bl.Code,
                    bl.Product?.Name ?? string.Empty,
                    received,
                    dispatched,
                    netQuantity);
            })
            .ToList();

        // --------------------------------------------------------
        // TOTALES DEL TURNO
        // --------------------------------------------------------

        var receivedQuantity =
            balances.Sum(x => x.ReceivedQuantity);

        var dispatchedQuantity =
            balances.Sum(x => x.DispatchedQuantity);

        var netQuantity =
            receivedQuantity - dispatchedQuantity;

        // --------------------------------------------------------
        // RESPUESTA
        // --------------------------------------------------------

        return new ShiftSummaryDto(
            shift.Id,
            shift.ShiftDate,
            shift.ShiftType,
            shift.Status,
            shift.StartedAt,
            shift.EndedAt,
            shift.ShipId,
            shift.Ship?.Name ?? string.Empty,
            receivedQuantity,
            dispatchedQuantity,
            netQuantity,
            balances);
    }
}
