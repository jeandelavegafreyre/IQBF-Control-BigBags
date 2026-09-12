using IQBF.Application.DTOs.Receptions;
namespace IQBF.Application.Interfaces;
public interface IReceptionService
{
    Task<ReceptionDto> CreateAsync(CreateReceptionRequest request, string actorUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ReceptionDto>> GetByShiftAsync(Guid shiftId, CancellationToken cancellationToken = default);
    Task<ReceptionDto> UpdateAsync(Guid id, CreateReceptionRequest request, string actorUid, CancellationToken cancellationToken = default);
}
