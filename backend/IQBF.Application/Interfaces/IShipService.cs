using IQBF.Application.DTOs.Ships;
namespace IQBF.Application.Interfaces;
public interface IShipService
{
    Task<IReadOnlyCollection<ShipDto>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ShipDto> CreateAsync(CreateShipRequest request, string actorUid, CancellationToken cancellationToken = default);
}
