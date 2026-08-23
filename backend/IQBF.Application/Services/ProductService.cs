using IQBF.Application.DTOs.Products;
using IQBF.Application.Interfaces;
using IQBF.Domain.Entities;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class ProductService : IProductService
{
    private readonly IQBFDbContext _db;
    public ProductService(IQBFDbContext db) => _db = db;

    public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Products.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new ProductDto(x.Id, x.Name, x.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, string actorUid, CancellationToken cancellationToken = default)
    {
        var name = (request.Name ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre del producto es obligatorio.");
        if (await _db.Products.AnyAsync(x => x.Name == name, cancellationToken))
            throw new InvalidOperationException("Ya existe un producto con ese nombre.");

        var entity = new Product { Name = name, IsActive = true, CreatedBy = actorUid };
        _db.Products.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return new ProductDto(entity.Id, entity.Name, entity.IsActive);
    }
}
