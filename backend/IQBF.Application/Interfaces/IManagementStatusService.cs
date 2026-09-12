using IQBF.Application.DTOs.Reports;

namespace IQBF.Application.Interfaces;

public interface IManagementStatusService
{
    Task<ManagementInProcessDto> GetInProcessAsync(CancellationToken cancellationToken = default);
}
