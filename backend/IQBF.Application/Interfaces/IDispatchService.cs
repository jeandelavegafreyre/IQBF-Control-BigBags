using IQBF.Application.DTOs.Dispatches;
namespace IQBF.Application.Interfaces;
public interface IDispatchService
{
    Task<DispatchDto> CreateAsync(CreateDispatchRequest request, string actorUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DispatchDto>> GetByShiftAsync(Guid shiftId, CancellationToken cancellationToken = default);
    Task<DispatchDto> UpdateAsync(Guid id, CreateDispatchRequest request, string actorUid, CancellationToken cancellationToken = default);
}
