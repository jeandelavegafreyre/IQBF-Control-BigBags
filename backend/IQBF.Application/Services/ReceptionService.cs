using System.Data;
using IQBF.Application.DTOs.Receptions;
using IQBF.Application.Interfaces;
using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class ReceptionService : IReceptionService
{
    private readonly IQBFDbContext _db;

    public ReceptionService(IQBFDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<ReceptionDto>> GetByShiftAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.Receptions
            .AsNoTracking()
            .Where(x => x.ShiftId == shiftId)
            .Include(x => x.Items)
                .ThenInclude(x => x.BL)
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.TransactionNumber)
            .ToListAsync(cancellationToken);

        return rows.Select(row => Map(row)).ToList();
    }

    public async Task<ReceptionDto> CreateAsync(
        CreateReceptionRequest request,
        string actorUid,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);
        var truck = request.TerminalTruck.Trim();
        var ids = request.Items.Select(x => x.BLId).Distinct().ToArray();

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var shift = await GetOpenShiftAsync(request.ShiftId, cancellationToken);
            var bls = await GetValidBLsAsync(ids, shift.ShipId, cancellationToken);
            await ValidateReceptionQuantitiesAsync(request.Items, bls, null, cancellationToken);

            var nextTransactionNumber = (await _db.Receptions
                .Where(x => x.ShiftId == request.ShiftId)
                .MaxAsync(x => (int?)x.TransactionNumber, cancellationToken) ?? 0) + 1;

            var entity = new Reception
            {
                ShiftId = request.ShiftId,
                TransactionNumber = nextTransactionNumber,
                TerminalTruck = truck,
                Comment = NormalizeComment(request.Comment),
                CreatedBy = actorUid,
                Items = request.Items.Select(x => new ReceptionItem
                {
                    BLId = x.BLId,
                    Quantity = x.Quantity,
                    CreatedBy = actorUid
                }).ToList()
            };

            _db.Receptions.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var codes = bls.ToDictionary(x => x.Id, x => x.Code);
            return Map(entity, codes);
        });
    }

    public async Task<ReceptionDto> UpdateAsync(
        Guid id,
        CreateReceptionRequest request,
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

            var entity = await _db.Receptions
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("Recepción no encontrada.");

            if (entity.ShiftId != request.ShiftId)
                throw new InvalidOperationException("No se puede mover una recepción a otro turno.");

            var shift = await GetOpenShiftAsync(entity.ShiftId, cancellationToken);
            var bls = await GetValidBLsAsync(ids, shift.ShipId, cancellationToken);
            await ValidateReceptionQuantitiesAsync(request.Items, bls, entity.Id, cancellationToken);

            _db.ReceptionItems.RemoveRange(entity.Items);
            entity.Items = request.Items.Select(x => new ReceptionItem
            {
                ReceptionId = entity.Id,
                BLId = x.BLId,
                Quantity = x.Quantity,
                CreatedBy = actorUid
            }).ToList();
            entity.TerminalTruck = request.TerminalTruck.Trim();
            entity.Comment = NormalizeComment(request.Comment);
            entity.UpdatedBy = actorUid;

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var codes = bls.ToDictionary(x => x.Id, x => x.Code);
            return Map(entity, codes);
        });
    }

    private static void ValidateRequest(CreateReceptionRequest request)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException("La recepción debe contener al menos un BL.");
        if (request.Items.Any(x => x.Quantity <= 0))
            throw new ArgumentException("Todas las cantidades deben ser mayores que cero.");
        if (!string.IsNullOrWhiteSpace(request.Comment) && request.Comment.Trim().Length > 100)
            throw new ArgumentException("El comentario no puede exceder 100 caracteres.");

        var truck = (request.TerminalTruck ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(truck) || !truck.All(char.IsDigit))
            throw new ArgumentException("Terminal Truck debe contener únicamente números.");

        var ids = request.Items.Select(x => x.BLId).Distinct().ToArray();
        if (ids.Length != request.Items.Count)
            throw new ArgumentException("No se puede repetir el mismo BL dentro de una recepción.");
    }

    private async Task<Shift> GetOpenShiftAsync(Guid shiftId, CancellationToken cancellationToken)
    {
        var shift = await _db.Shifts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == shiftId, cancellationToken)
            ?? throw new KeyNotFoundException("Turno no encontrado.");
        if (shift.Status != ShiftStatus.Open)
            throw new InvalidOperationException("No se pueden modificar recepciones de un turno cerrado.");
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

    private async Task ValidateReceptionQuantitiesAsync(
        IReadOnlyCollection<ReceptionItemRequest> items,
        IReadOnlyCollection<BL> bls,
        Guid? excludingReceptionId,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var bl = bls.First(x => x.Id == item.BLId);
            var query = _db.ReceptionItems.Where(x => x.BLId == item.BLId);
            if (excludingReceptionId.HasValue)
                query = query.Where(x => x.ReceptionId != excludingReceptionId.Value);

            var totalReceived = await query.SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0;
            var available = bl.TotalQuantity - totalReceived;
            if (item.Quantity > available)
                throw new InvalidOperationException($"El BL {bl.Code} excede la cantidad declarada. Disponible para esta corrección: {available:N0}.");
        }
    }

    private static string? NormalizeComment(string? comment) =>
        string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();

    private static ReceptionDto Map(Reception entity, IReadOnlyDictionary<Guid, string>? codes = null) =>
        new(
            entity.Id,
            entity.ShiftId,
            entity.TransactionNumber,
            entity.TerminalTruck,
            entity.Comment,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy,
            entity.Items.Select(x => new ReceptionItemDto(
                x.BLId,
                codes is not null ? codes[x.BLId] : x.BL?.Code ?? string.Empty,
                x.Quantity)).ToList());
}
