using IQBF.Application.DTOs.Dashboard;
using IQBF.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // ============================================================
    // RESUMEN GENERAL POR NAVE
    // ============================================================

    [HttpGet("ships/{shipId:guid}/summary")]
    [ProducesResponseType(typeof(ShipSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShipSummaryDto>> GetShipSummary(
        Guid shipId,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetShipSummaryAsync(
            shipId,
            cancellationToken);

        return Ok(result);
    }

    // ============================================================
    // RESUMEN POR TURNO
    // ============================================================

    [HttpGet("shifts/{shiftId:guid}/summary")]
    [ProducesResponseType(typeof(ShiftSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ShiftSummaryDto>> GetShiftSummary(
        Guid shiftId,
        CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetShiftSummaryAsync(
            shiftId,
            cancellationToken);

        return Ok(result);
    }
}