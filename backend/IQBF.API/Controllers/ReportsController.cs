using IQBF.Application.DTOs.Reports;
using IQBF.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IOperationalReportService _reportService;
    private readonly IManagementStatusService _managementStatusService;

    public ReportsController(IOperationalReportService reportService, IManagementStatusService managementStatusService)
    {
        _reportService = reportService;
        _managementStatusService = managementStatusService;
    }

    [HttpGet("shifts/{shiftId:guid}/movements")]
    public async Task<ActionResult<IReadOnlyList<OperationalMovementDto>>> GetShiftMovements(Guid shiftId, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetShiftMovementsAsync(shiftId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("management")]
    [Authorize(Roles = "Administrator,Management")]
    public async Task<ActionResult<ManagementReportDto>> GetManagementReport([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetManagementReportAsync(year, month, cancellationToken);
        return Ok(result);
    }

    [HttpGet("management/active-ships")]
    [Authorize(Roles = "Administrator,Management")]
    public async Task<ActionResult<ManagementInProcessDto>> GetManagementActiveShips(CancellationToken cancellationToken)
    {
        var result = await _managementStatusService.GetInProcessAsync(cancellationToken);
        return Ok(result);
    }
}
