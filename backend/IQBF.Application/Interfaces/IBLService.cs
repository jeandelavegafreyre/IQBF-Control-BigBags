using IQBF.Application.DTOs.BLs;
namespace IQBF.Application.Interfaces;
public interface IBLService
{
    Task<IReadOnlyCollection<BLDto>> GetByShipAsync(Guid shipId, CancellationToken cancellationToken = default);
    Task<BLDto> CreateAsync(CreateBLRequest request, string actorUid, CancellationToken cancellationToken = default);
}
