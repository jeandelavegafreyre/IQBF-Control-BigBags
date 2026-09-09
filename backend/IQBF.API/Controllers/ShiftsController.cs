using IQBF.Application.DTOs.Shifts;
using IQBF.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/shifts")]
[Authorize(Roles = "Administrator,Yard")]
public class ShiftsController : ControllerBase
{
    private readonly IShiftService _service;
    public ShiftsController(IShiftService service) => _service = service;

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen([FromQuery] Guid shipId, CancellationToken cancellationToken)
    {
        var shift = await _service.GetOpenAsync(shipId, cancellationToken);
        return shift is null ? NoContent() : Ok(shift);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start(StartShiftRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.StartAsync(request, User.Identity!.Name!, cancellationToken));

    [HttpPost("{shiftId:guid}/close")]
    public async Task<IActionResult> Close(Guid shiftId, CancellationToken cancellationToken)
    {
        await _service.CloseAsync(shiftId, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }
}
