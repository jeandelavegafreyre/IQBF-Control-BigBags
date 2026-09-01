using IQBF.Application.DTOs.Dashboard;

namespace IQBF.Application.Interfaces;

public interface IDashboardService
{
    Task<ShipSummaryDto> GetShipSummaryAsync(
        Guid shipId,
        CancellationToken cancellationToken = default);

    Task<ShiftSummaryDto> GetShiftSummaryAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default);
}