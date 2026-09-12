using IQBF.Application.DTOs.Reports;

namespace IQBF.Application.Interfaces;

public interface IOperationalReportService
{
    Task<IReadOnlyList<OperationalMovementDto>> GetShiftMovementsAsync(
        Guid shiftId,
        CancellationToken cancellationToken = default);

    Task<ManagementReportDto> GetManagementReportAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default);
}
