using IQBF.Application.DTOs.Dispatches;
namespace IQBF.Application.Interfaces;
public interface IDispatchService
{
    Task<DispatchDto> CreateAsync(CreateDispatchRequest request, string actorUid, CancellationToken cancellationToken = default);
}
