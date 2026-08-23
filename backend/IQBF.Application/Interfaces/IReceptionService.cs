using IQBF.Application.DTOs.Receptions;
namespace IQBF.Application.Interfaces;
public interface IReceptionService
{
    Task<ReceptionDto> CreateAsync(CreateReceptionRequest request, string actorUid, CancellationToken cancellationToken = default);
}
