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

    public async Task<ReceptionDto> CreateAsync(
        CreateReceptionRequest request,
        string actorUid,
        CancellationToken cancellationToken = default)
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

        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var shift = await _db.Shifts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ShiftId, cancellationToken)
                ?? throw new KeyNotFoundException("Turno no encontrado.");

            if (shift.Status != ShiftStatus.Open)
                throw new InvalidOperationException("No se pueden registrar recepciones en un turno cerrado.");

            var bls = await _db.BLs
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (bls.Count != ids.Length)
                throw new KeyNotFoundException("Uno o más BL no existen.");
            if (bls.Any(x => x.ShipId != shift.ShipId))
                throw new InvalidOperationException("Todos los BL deben pertenecer a la nave del turno.");
            if (bls.Any(x => !x.IsActive))
                throw new InvalidOperationException("No se puede operar con un BL inactivo.");

            foreach (var item in request.Items)
            {
                var bl = bls.First(x => x.Id == item.BLId);
                var totalReceived = await _db.ReceptionItems
                    .Where(x => x.BLId == item.BLId)
                    .SumAsync(x => (int?)x.Quantity, cancellationToken) ?? 0;

                var available = bl.TotalQuantity - totalReceived;
                var newTotal = totalReceived + item.Quantity;

                if (newTotal > bl.TotalQuantity)
                {
                    throw new InvalidOperationException(
                        $"El BL {bl.Code} excede la cantidad declarada. " +
                        $"Declarado: {bl.TotalQuantity:N0}. " +
                        $"Recibido: {totalReceived:N0}. " +
                        $"Intento: {item.Quantity:N0}. " +
                        $"Disponible: {available:N0}.");
                }
            }

            var nextTransactionNumber = (await _db.Receptions
                .Where(x => x.ShiftId == request.ShiftId)
                .MaxAsync(x => (int?)x.TransactionNumber, cancellationToken) ?? 0) + 1;

            var entity = new Reception
            {
                ShiftId = request.ShiftId,
                TransactionNumber = nextTransactionNumber,
                TerminalTruck = truck,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
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
            return new ReceptionDto(
                entity.Id,
                entity.ShiftId,
                entity.TransactionNumber,
                entity.TerminalTruck,
                entity.Comment,
                entity.CreatedAt,
                entity.CreatedBy,
                entity.Items.Select(x => new ReceptionItemDto(x.BLId, codes[x.BLId], x.Quantity)).ToList());
        });
    }
}
