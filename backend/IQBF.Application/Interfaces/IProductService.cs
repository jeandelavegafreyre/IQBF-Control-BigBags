using IQBF.Application.DTOs.Products;
namespace IQBF.Application.Interfaces;
public interface IProductService
{
    Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, string actorUid, CancellationToken cancellationToken = default);
}
