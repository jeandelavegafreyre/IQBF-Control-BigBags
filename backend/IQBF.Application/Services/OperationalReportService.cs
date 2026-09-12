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
}
