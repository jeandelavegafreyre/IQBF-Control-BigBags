using System.Data;
using IQBF.Application.DTOs.Dispatches;
using IQBF.Application.Interfaces;
using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class DispatchService : IDispatchService
{
    private readonly IQBFDbContext _db;

    public DispatchService(IQBFDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<DispatchDto>> GetByShiftAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.Dispatches
            .AsNoTracking()
            .Where(x => x.ShiftId == shiftId)
            .Include(x => x.Items)
                .ThenInclude(x => x.BL)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.TransactionNumber)
            .ToListAsync(cancellationToken);

        return rows.Select(row => Map(row)).ToList();
    }

    public async Task<DispatchDto> CreateAsync(
        CreateDispatchRequest request,
        string actorUid,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var plate = request.Plate.Trim().ToUpperInvariant();
        var ids = request.Items.Select(x => x.BLId).Distinct().ToArray();

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var shift = await GetOpenShiftAsync(request.ShiftId, cancellationToken);
            var bls = await GetValidBLsAsync(ids, shift.ShipId, cancellationToken);
            await ValidateDispatchQuantitiesAsync(request.Items, bls, null, cancellationToken);

            var nextTransactionNumber = (await _db.Dispatches
                .Where(x => x.ShiftId == request.ShiftId)
                .MaxAsync(x => (int?)x.TransactionNumber, cancellationToken) ?? 0) + 1;

            var entity = new Dispatch
            {
                ShiftId = request.ShiftId,
                TransactionNumber = nextTransactionNumber,
                Plate = plate,
                Comment = NormalizeComment(request.Comment),
                CreatedBy = actorUid,
                Items = request.Items.Select(x => new DispatchItem
                {
                    BLId = x.BLId,
                    Quantity = x.Quantity,
                    CreatedBy = actorUid
                }).ToList()
            };

            _db.Dispatches.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var codes = bls.ToDictionary(x => x.Id, x => x.Code);
            return Map(entity, codes);
        });
    }

    public async Task<DispatchDto> UpdateAsync(
        Guid id,
        CreateDispatchRequest request,
        string actorUid,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var ids = request.Items.Select(x => x.BLId).Distinct().ToArray();

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var entity = await _db.Dispatches
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Despacho no encontrado.");

            if (entity.ShiftId != request.ShiftId)
                throw new InvalidOperationException("No se puede mover un despacho a otro turno.");

            var shift = await GetOpenShiftAsync(entity.ShiftId, cancellationToken);
            var bls = await GetValidBLsAsync(ids, shift.ShipId, cancellationToken);
            await ValidateDispatchQuantitiesAsync(request.Items, bls, entity.Id, cancellationToken);

            await _db.DispatchItems
                .Where(x => x.DispatchId == entity.Id)
                .ExecuteDeleteAsync(cancellationToken);

            var newItems = request.Items.Select(x => new DispatchItem
            {
                DispatchId = entity.Id,
                BLId = x.BLId,
                Quantity = x.Quantity,
                CreatedBy = actorUid
            }).ToList();

            _db.DispatchItems.AddRange(newItems);
            entity.Items = newItems;
            entity.Plate = request.Plate.Trim().ToUpperInvariant();
            entity.Comment = NormalizeComment(request.Comment);
            entity.UpdatedBy = actorUid;

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var codes = bls.ToDictionary(x => x.Id, x => x.Code);
            return Map(entity, codes);
        });
    }

    private static void ValidateRequest(CreateDispatchRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException("El despacho debe contener al menos un BL.");
        if (request.Items.Any(x => x.Quantity <= 0))
            throw new ArgumentException("Todas las cantidades deben ser mayores que cero.");
        if (!string.IsNullOrWhiteSpace(request.Comment) && request.Comment.Trim().Length > 100)
            throw new ArgumentException("El comentario no puede exceder 100 caracteres.");

        var plate = (request.Plate ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(plate))
            throw new ArgumentException("La placa es obligatoria.");

        var ids = request.Items.Select(x => x.BLId).Distinct().ToArray();
        if (ids.Length != request.Items.Count)
            throw new ArgumentException("No se puede repetir el mismo BL dentro de un despacho.");
    }

    private async Task<Shift> GetOpenShiftAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        var shift = await _db.Shifts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == shiftId, cancellationToken)
            ?? throw new KeyNotFoundException("Turno no encontrado.");
        if (shift.Status != ShiftStatus.Open)
            throw new InvalidOperationException("No se pueden modificar despachos de un turno cerrado.");
        return shift;
    }

    private async Task<List<BL>> GetValidBLsAsync(Guid[] ids, Guid shipId, CancellationToken cancellationToken)
    {
        var bls = await _db.BLs.Where(x => ids.Contains(x.Id)).ToListAsync(cancellationToken);
        if (bls.Count != ids.Length)
            throw new KeyNotFoundException("Uno o más BL no existen.");
        if (bls.Any(x => x.ShipId != shipId))
            throw new InvalidOperationException("Todos los BL deben pertenecer a la nave del turno.");
        if (bls.Any(x => !x.IsActive))
            throw new InvalidOperationException("No se puede operar con un BL inactivo.");
        return bls;
    }

    private async Task ValidateDispatchQuantitiesAsync(
        IReadOnlyCollection<DispatchItemRequest> items,
        IReadOnlyCollection<BL> bls,
        Guid? excludingDispatchId,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var bl = bls.First(x => x.Id == item.BLId);
            var totalReceived = await _db.ReceptionItems
                .Where(x => x.BLId == item.BLId)
                .SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0;

            var dispatchQuery = _db.DispatchItems.Where(x => x.BLId == item.BLId);
            if (excludingDispatchId.HasValue)
                dispatchQuery = dispatchQuery.Where(x => x.DispatchId != excludingDispatchId.Value);

            var totalDispatched = await dispatchQuery.SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0;
            var available = totalReceived - totalDispatched;

            if (item.Quantity > available)
                throw new InvalidOperationException($"Saldo insuficiente para el BL {bl.Code}. Disponible para esta corrección: {available:N0}.");
        }
    }

    private static string? NormalizeComment(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

    private static DispatchDto Map(Dispatch entity, IReadOnlyDictionary<Guid, string>? codes = null) =>
        new(
            entity.Id,
            entity.ShiftId,
            entity.TransactionNumber,
            entity.Plate,
            entity.Comment,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy,
            entity.Items.Select(x => new DispatchItemDto(
                x.BLId,
                codes is not null ? codes[x.BLId] : x.BL?.Code ?? string.Empty,
                x.Quantity)).ToList());
}
