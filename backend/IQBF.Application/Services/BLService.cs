using IQBF.Application.DTOs.BLs;
using IQBF.Application.Interfaces;
using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class BLService : IBLService
{
    private readonly IQBFDbContext _db;
    public BLService(IQBFDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<BLDto>> GetByShipAsync(Guid shipId, CancellationToken cancellationToken = default) =>
        await _db.BLs.AsNoTracking()
            .Where(x => x.ShipId == shipId && x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new BLDto(x.Id, x.Code, x.TotalQuantity, x.IsActive, x.ShipId, x.Ship!.Name, x.ProductId, x.Product!.Name))
            .ToListAsync(cancellationToken);

    public async Task<BLDto> CreateAsync(CreateBLRequest request, string actorUid, CancellationToken cancellationToken = default)
    {
        var code = (request.Code ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("El código BL es obligatorio.");
        if (request.TotalQuantity <= 0) throw new ArgumentException("La cantidad total del BL debe ser mayor que cero.");

        var ship = await _db.Ships.FirstOrDefaultAsync(x => x.Id == request.ShipId, cancellationToken)
            ?? throw new KeyNotFoundException("Nave no encontrada.");
        if (ship.Status != ShipStatus.Active) throw new InvalidOperationException("El BL solo puede registrarse para una nave activa.");

        var product = await _db.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken)
            ?? throw new KeyNotFoundException("Producto no encontrado.");
        if (!product.IsActive) throw new InvalidOperationException("El producto seleccionado está inactivo.");

        if (await _db.BLs.AnyAsync(x => x.ShipId == request.ShipId && x.Code == code, cancellationToken))
            throw new InvalidOperationException("Ese BL ya existe para la nave seleccionada.");

        var entity = new BL { Code = code, TotalQuantity = request.TotalQuantity, ShipId = request.ShipId, ProductId = request.ProductId, IsActive = true, CreatedBy = actorUid };
        _db.BLs.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return new BLDto(entity.Id, entity.Code, entity.TotalQuantity, entity.IsActive, ship.Id, ship.Name, product.Id, product.Name);
    }
}
