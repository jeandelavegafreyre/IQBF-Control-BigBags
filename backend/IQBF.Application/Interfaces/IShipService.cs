using IQBF.Application.DTOs.Ships;
namespace IQBF.Application.Interfaces;
public interface IShipService
{
    Task<IReadOnlyCollection<ShipDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ShipDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ShipDto> CreateAsync(CreateShipRequest request, string actorUid, CancellationToken cancellationToken = default);
    Task<ShipDto> UpdateStatusAsync(Guid shipId, UpdateShipStatusRequest request, string actorUid, CancellationToken cancellationToken = default);
}
