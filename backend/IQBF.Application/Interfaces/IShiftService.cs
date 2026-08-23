using IQBF.Application.DTOs.Shifts;
namespace IQBF.Application.Interfaces;
public interface IShiftService
{
    Task<ShiftDto> StartAsync(StartShiftRequest request, string actorUid, CancellationToken cancellationToken = default);
    Task CloseAsync(Guid shiftId, string actorUid, CancellationToken cancellationToken = default);
}
