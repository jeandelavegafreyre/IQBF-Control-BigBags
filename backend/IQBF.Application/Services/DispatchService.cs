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

    public async Task<DispatchDto> CreateAsync(
        CreateDispatchRequest request,
        string actorUid,
        CancellationToken cancellationToken = default)
    {
        if (request.Items is null || request.Items.Count == 0)
            throw new ArgumentException(
                "El despacho debe contener al menos un BL.");

        if (request.Items.Any(x => x.Quantity <= 0))
            throw new ArgumentException(
                "Todas las cantidades deben ser mayores que cero.");

        if (!string.IsNullOrWhiteSpace(request.Comment) &&
            request.Comment.Trim().Length > 100)
            throw new ArgumentException(
                "El comentario no puede exceder 100 caracteres.");

        var plate = (request.Plate ?? string.Empty)
            .Trim()
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(plate))
            throw new ArgumentException(
                "La placa es obligatoria.");

        // =====================================================
        // VALIDAR TURNO
        // =====================================================

        var shift = await _db.Shifts
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.ShiftId,
                cancellationToken)
            ?? throw new KeyNotFoundException(
                "Turno no encontrado.");

        if (shift.Status != ShiftStatus.Open)
        {
            throw new InvalidOperationException(
                "No se pueden registrar despachos en un turno cerrado.");
        }

        // =====================================================
        // VALIDAR BLs
        // =====================================================

        var ids = request.Items
            .Select(x => x.BLId)
            .Distinct()
            .ToArray();

        if (ids.Length != request.Items.Count)
        {
            throw new ArgumentException(
                "No se puede repetir el mismo BL dentro de un despacho.");
        }

        var bls = await _db.BLs
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (bls.Count != ids.Length)
        {
            throw new KeyNotFoundException(
                "Uno o más BL no existen.");
        }

        if (bls.Any(x => x.ShipId != shift.ShipId))
        {
            throw new InvalidOperationException(
                "Todos los BL deben pertenecer a la nave del turno.");
        }

        if (bls.Any(x => !x.IsActive))
        {
            throw new InvalidOperationException(
                "No se puede operar con un BL inactivo.");
        }

        // =====================================================
        // CORRELATIVO DE DESPACHO POR TURNO
        // =====================================================

        var nextTransactionNumber =
            (await _db.Dispatches
                .Where(x => x.ShiftId == request.ShiftId)
                .MaxAsync(
                    x => (int?)x.TransactionNumber,
                    cancellationToken) ?? 0) + 1;

        // =====================================================
        // CREAR DESPACHO
        // =====================================================

        var entity = new Dispatch
        {
            ShiftId = request.ShiftId,

            TransactionNumber = nextTransactionNumber,

            Plate = plate,

            Comment = string.IsNullOrWhiteSpace(request.Comment)
                ? null
                : request.Comment.Trim(),

            CreatedBy = actorUid,

            Items = request.Items
                .Select(x => new DispatchItem
                {
                    BLId = x.BLId,
                    Quantity = x.Quantity,
                    CreatedBy = actorUid
                })
                .ToList()
        };

        _db.Dispatches.Add(entity);

        await _db.SaveChangesAsync(cancellationToken);

        // =====================================================
        // RESPUESTA
        // =====================================================

        var codes = bls.ToDictionary(
            x => x.Id,
            x => x.Code);

        return new DispatchDto(
            entity.Id,
            entity.ShiftId,
            entity.TransactionNumber,
            entity.Plate,
            entity.Comment,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.Items
                .Select(x => new DispatchItemDto(
                    x.BLId,
                    codes[x.BLId],
                    x.Quantity))
                .ToList());
    }
}
