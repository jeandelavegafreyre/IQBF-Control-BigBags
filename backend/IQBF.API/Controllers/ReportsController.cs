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

    public ReportsController(IOperationalReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("shifts/{shiftId:guid}/movements")]
    [ProducesResponseType(typeof(IReadOnlyList<OperationalMovementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<OperationalMovementDto>>> GetShiftMovements(
        Guid shiftId,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetShiftMovementsAsync(shiftId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("management")]
    [Authorize(Roles = "Administrator,Management")]
    [ProducesResponseType(typeof(ManagementReportDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ManagementReportDto>> GetManagementReport(
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetManagementReportAsync(year, month, cancellationToken);
        return Ok(result);
    }
}
