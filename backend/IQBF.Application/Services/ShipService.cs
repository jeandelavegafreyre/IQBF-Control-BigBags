using IQBF.Application.DTOs.Ships;
using IQBF.Application.Interfaces;
using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class ShipService : IShipService
{
    private readonly IQBFDbContext _db;
    public ShipService(IQBFDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<ShipDto>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await _db.Ships.AsNoTracking()
            .Where(x => x.Status == ShipStatus.Active)
            .OrderBy(x => x.Name)
            .Select(x => new ShipDto(x.Id, x.Name, x.Status))
            .ToListAsync(cancellationToken);

    public async Task<ShipDto> CreateAsync(CreateShipRequest request, string actorUid, CancellationToken cancellationToken = default)
    {
        var name = (request.Name ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre de la nave es obligatorio.");
        if (await _db.Ships.AnyAsync(x => x.Name == name, cancellationToken))
            throw new InvalidOperationException("Ya existe una nave con ese nombre.");

        var entity = new Ship { Name = name, Status = ShipStatus.Active, CreatedBy = actorUid };
        _db.Ships.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new ShipDto(entity.Id, entity.Name, entity.Status);
    }
}
