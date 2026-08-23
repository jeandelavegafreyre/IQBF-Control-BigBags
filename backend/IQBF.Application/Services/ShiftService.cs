using IQBF.Application.DTOs.Shifts;
using IQBF.Application.Interfaces;
using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class ShiftService : IShiftService
{
    private readonly IQBFDbContext _db;
    public ShiftService(IQBFDbContext db) => _db = db;

    public async Task<ShiftDto> StartAsync(StartShiftRequest request, string actorUid, CancellationToken cancellationToken = default)
    {
        var ship = await _db.Ships.FirstOrDefaultAsync(x => x.Id == request.ShipId, cancellationToken)
            ?? throw new KeyNotFoundException("Nave no encontrada.");
        if (ship.Status != ShipStatus.Active) throw new InvalidOperationException("Solo se puede iniciar control sobre una nave activa.");

        var exists = await _db.Shifts.AnyAsync(x =>
            x.ShipId == request.ShipId && x.ShiftDate == request.ShiftDate && x.ShiftType == request.ShiftType,
            cancellationToken);
        if (exists) throw new InvalidOperationException("Ya existe un turno para esa nave, fecha y tipo.");

        var entity = new Shift
        {
            ShipId = request.ShipId,
            ShiftDate = request.ShiftDate,
            ShiftType = request.ShiftType,
            Status = ShiftStatus.Open,
            StartedAt = DateTime.UtcNow,
            CreatedBy = actorUid
        };
        _db.Shifts.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new ShiftDto(entity.Id, entity.ShiftDate, entity.ShiftType, entity.Status, entity.StartedAt, entity.EndedAt, ship.Id, ship.Name);
    }

    public async Task CloseAsync(Guid shiftId, string actorUid, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Shifts.FirstOrDefaultAsync(x => x.Id == shiftId, cancellationToken)
            ?? throw new KeyNotFoundException("Turno no encontrado.");

        if (entity.Status == ShiftStatus.Closed) return;
        entity.Status = ShiftStatus.Closed;
        entity.EndedAt = DateTime.UtcNow;
        entity.UpdatedBy = actorUid;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
