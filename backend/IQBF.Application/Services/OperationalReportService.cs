using IQBF.Application.DTOs.Reports;
using IQBF.Application.Interfaces;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public sealed class OperationalReportService : IOperationalReportService
{
    private readonly IQBFDbContext _dbContext;

    public OperationalReportService(IQBFDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OperationalMovementDto>> GetShiftMovementsAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        var shiftExists = await _dbContext.Shifts
            .AsNoTracking()
            .AnyAsync(x => x.Id == shiftId, cancellationToken);

        if (!shiftExists)
            throw new KeyNotFoundException("No se encontró el turno solicitado.");

        var receptions = await _dbContext.Receptions
            .AsNoTracking()
            .Where(x => x.ShiftId == shiftId)
            .Include(x => x.Items)
                .ThenInclude(x => x.BL)
                    .ThenInclude(x => x!.Product)
            .Include(x => x.Photos)
            .ToListAsync(cancellationToken);

        var dispatches = await _dbContext.Dispatches
            .AsNoTracking()
            .Where(x => x.ShiftId == shiftId)
            .Include(x => x.Items)
                .ThenInclude(x => x.BL)
                    .ThenInclude(x => x!.Product)
            .Include(x => x.Photos)
            .ToListAsync(cancellationToken);

        var operatorUids = receptions
            .SelectMany(x => new[] { x.CreatedBy, x.UpdatedBy })
            .Concat(dispatches.SelectMany(x => new[] { x.CreatedBy, x.UpdatedBy }))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var operatorNames = operatorUids.Count == 0
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : (await _dbContext.Users
                .AsNoTracking()
                .Where(x => operatorUids.Contains(x.UID))
                .Select(x => new { x.UID, x.FirstName, x.LastName })
                .ToListAsync(cancellationToken))
                .ToDictionary(
                    x => x.UID,
                    x => $"{x.FirstName} {x.LastName}".Trim(),
                    StringComparer.OrdinalIgnoreCase);

        string? ResolveOperatorName(string? uid)
        {
            if (string.IsNullOrWhiteSpace(uid)) return uid;
            return operatorNames.TryGetValue(uid.Trim(), out var fullName) && !string.IsNullOrWhiteSpace(fullName)
                ? fullName
                : uid;
        }

        var movements = new List<OperationalMovementDto>(receptions.Count + dispatches.Count);

        movements.AddRange(receptions.Select(reception => new OperationalMovementDto(
            reception.Id,
            "Reception",
            reception.TransactionNumber,
            reception.CreatedAt,
            ResolveOperatorName(reception.CreatedBy),
            reception.UpdatedAt,
            ResolveOperatorName(reception.UpdatedBy),
            reception.TerminalTruck,
            reception.Comment,
            reception.Items
                .OrderBy(item => item.BL!.Code)
                .Select(item => new OperationalMovementItemDto(
                    item.BLId,
                    item.BL!.Code,
                    item.BL.Product?.Name ?? string.Empty,
                    item.Quantity))
                .ToList(),
            reception.Photos
                .OrderBy(photo => photo.CreatedAt)
                .Select(photo => new OperationalMovementPhotoDto(
                    photo.Id,
                    photo.PhotoUrl,
                    photo.FileName,
                    photo.ContentType,
                    photo.FileSize))
                .ToList())));

        movements.AddRange(dispatches.Select(dispatch => new OperationalMovementDto(
            dispatch.Id,
            "Dispatch",
            dispatch.TransactionNumber,
            dispatch.CreatedAt,
            ResolveOperatorName(dispatch.CreatedBy),
            dispatch.UpdatedAt,
            ResolveOperatorName(dispatch.UpdatedBy),
            dispatch.Plate,
            dispatch.Comment,
            dispatch.Items
                .OrderBy(item => item.BL!.Code)
                .Select(item => new OperationalMovementItemDto(
                    item.BLId,
                    item.BL!.Code,
                    item.BL.Product?.Name ?? string.Empty,
                    item.Quantity))
                .ToList(),
            dispatch.Photos
                .OrderBy(photo => photo.CreatedAt)
                .Select(photo => new OperationalMovementPhotoDto(
                    photo.Id,
                    photo.PhotoUrl,
                    photo.FileName,
                    photo.ContentType,
                    photo.FileSize))
                .ToList())));

        return movements
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.TransactionNumber)
            .ToList();
    }

    public async Task<ManagementReportDto> GetManagementReportAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("El año solicitado no es válido.");
        if (month < 1 || month > 12)
            throw new ArgumentException("El mes solicitado no es válido.");

        // CreatedAt se almacena en UTC. Para los reportes operativos se clasifica por hora de Perú (UTC-5).
        static DateTime PeruTime(DateTime utc) => utc.AddHours(-5);

        var dispatchTimeline = await _dbContext.Dispatches
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.CreatedAt,
                ShipId = x.Shift!.ShipId,
                ShipName = x.Shift.Ship!.Name
            })
            .ToListAsync(cancellationToken);

        var lastDispatchByShip = dispatchTimeline
            .GroupBy(x => new { x.ShipId, x.ShipName })
            .Select(group => new
            {
                group.Key.ShipId,
                group.Key.ShipName,
                LastDispatchAt = group.Max(x => x.CreatedAt)
            })
            .ToList();

        var receptionQuantityByShipAll = await _dbContext.ReceptionItems
            .AsNoTracking()
            .Select(x => new { ShipId = x.BL!.ShipId, x.Quantity })
            .ToListAsync(cancellationToken);

        var dispatchQuantityByShipAll = await _dbContext.DispatchItems
            .AsNoTracking()
            .Select(x => new { ShipId = x.BL!.ShipId, x.Quantity })
            .ToListAsync(cancellationToken);

        var receptionQuantityLookup = receptionQuantityByShipAll
            .GroupBy(x => x.ShipId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

        var dispatchQuantityLookup = dispatchQuantityByShipAll
            .GroupBy(x => x.ShipId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.Quantity));

        // Una nave se considera completada solo si tuvo recepción y todo lo recibido fue despachado.
        // El mes gerencial se define por la fecha de su último despacho global.
        var completedShips = lastDispatchByShip
            .Where(x =>
            {
                var received = receptionQuantityLookup.GetValueOrDefault(x.ShipId);
                var dispatched = dispatchQuantityLookup.GetValueOrDefault(x.ShipId);
                return received > 0 && received == dispatched;
            })
            .ToList();

        var completedInMonth = completedShips
            .Where(x =>
            {
                var local = PeruTime(x.LastDispatchAt);
                return local.Year == year && local.Month == month;
            })
            .OrderByDescending(x => x.LastDispatchAt)
            .ToList();

        var targetShipIds = completedInMonth.Select(x => x.ShipId).Distinct().ToList();

        var yearlyTrend = completedShips
            .Where(x => PeruTime(x.LastDispatchAt).Year == year)
            .GroupBy(x => PeruTime(x.LastDispatchAt).Month)
            .ToDictionary(x => x.Key, x => x.Select(y => y.ShipId).Distinct().Count());

        var trend = Enumerable.Range(1, 12)
            .Select(trendMonth =>
            {
                var shipIds = completedShips
                    .Where(x =>
                    {
                        var local = PeruTime(x.LastDispatchAt);
                        return local.Year == year && local.Month == trendMonth;
                    })
                    .Select(x => x.ShipId)
                    .Distinct()
                    .ToList();
                return new ManagementMonthlyTrendDto(
                    trendMonth,
                    yearlyTrend.GetValueOrDefault(trendMonth),
                    shipIds.Sum(id => dispatchQuantityLookup.GetValueOrDefault(id)));
            })
            .ToList();

        if (targetShipIds.Count == 0)
            return new ManagementReportDto(year, month, 0, 0, 0, 0, [], trend, []);

        var blRows = await _dbContext.BLs
            .AsNoTracking()
            .Where(x => targetShipIds.Contains(x.ShipId))
            .Select(x => new
            {
                x.Id,
                x.ShipId,
                x.TotalQuantity,
                ProductName = x.Product!.Name
            })
            .ToListAsync(cancellationToken);

        var receptionRows = await _dbContext.Receptions
            .AsNoTracking()
            .Where(x => targetShipIds.Contains(x.Shift!.ShipId))
            .Select(x => new { x.Id, x.CreatedAt, ShipId = x.Shift!.ShipId })
            .ToListAsync(cancellationToken);

        var dispatchRows = dispatchTimeline.Where(x => targetShipIds.Contains(x.ShipId)).ToList();

        var receptionItems = await _dbContext.ReceptionItems
            .AsNoTracking()
            .Where(x => targetShipIds.Contains(x.BL!.ShipId))
            .Select(x => new
            {
                x.BLId,
                ShipId = x.BL!.ShipId,
                ProductName = x.BL.Product!.Name,
                x.Quantity
            })
            .ToListAsync(cancellationToken);

        var dispatchItems = await _dbContext.DispatchItems
            .AsNoTracking()
            .Where(x => targetShipIds.Contains(x.BL!.ShipId))
            .Select(x => new
            {
                x.BLId,
                ShipId = x.BL!.ShipId,
                ProductName = x.BL.Product!.Name,
                x.Quantity
            })
            .ToListAsync(cancellationToken);

        var shipRows = completedInMonth.Select(ship =>
        {
            var bls = blRows.Where(x => x.ShipId == ship.ShipId).ToList();
            var receptionsForShip = receptionRows.Where(x => x.ShipId == ship.ShipId).ToList();
            var dispatchesForShip = dispatchRows.Where(x => x.ShipId == ship.ShipId).ToList();
            var received = receptionItems.Where(x => x.ShipId == ship.ShipId).Sum(x => x.Quantity);
            var dispatched = dispatchItems.Where(x => x.ShipId == ship.ShipId).Sum(x => x.Quantity);
            var firstReception = receptionsForShip.Count == 0 ? (DateTime?)null : receptionsForShip.Min(x => x.CreatedAt);
            var durationHours = firstReception.HasValue ? Math.Max(0, (ship.LastDispatchAt - firstReception.Value).TotalHours) : 0;

            return new ManagementShipDto(
                ship.ShipId,
                ship.ShipName,
                firstReception,
                ship.LastDispatchAt,
                bls.Sum(x => x.TotalQuantity),
                received,
                dispatched,
                received - dispatched,
                receptionsForShip.Count,
                dispatchesForShip.Count,
                Math.Round(durationHours, 1),
                bls.Count,
                bls.Select(x => x.ProductName).Distinct().OrderBy(x => x).ToList());
        }).ToList();

        var products = blRows
            .GroupBy(x => x.ProductName)
            .Select(group =>
            {
                var blIds = group.Select(x => x.Id).ToHashSet();
                return new ManagementProductDto(
                    group.Key,
                    group.Sum(x => x.TotalQuantity),
                    receptionItems.Where(x => blIds.Contains(x.BLId)).Sum(x => x.Quantity),
                    dispatchItems.Where(x => blIds.Contains(x.BLId)).Sum(x => x.Quantity),
                    group.Select(x => x.ShipId).Distinct().Count());
            })
            .OrderByDescending(x => x.DispatchedQuantity)
            .ToList();

        return new ManagementReportDto(
            year,
            month,
            shipRows.Count,
            shipRows.Sum(x => x.ReceivedQuantity),
            shipRows.Sum(x => x.DispatchedQuantity),
            shipRows.Sum(x => x.DeclaredQuantity),
            shipRows,
            trend,
            products);
    }
}
