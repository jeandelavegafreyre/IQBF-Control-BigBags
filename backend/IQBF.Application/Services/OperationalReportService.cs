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
        {
            throw new KeyNotFoundException("No se encontró el turno solicitado.");
        }

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

        var movements = new List<OperationalMovementDto>(receptions.Count + dispatches.Count);

        movements.AddRange(receptions.Select(reception => new OperationalMovementDto(
            reception.Id,
            "Reception",
            reception.TransactionNumber,
            reception.CreatedAt,
            reception.CreatedBy,
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
            dispatch.CreatedBy,
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
