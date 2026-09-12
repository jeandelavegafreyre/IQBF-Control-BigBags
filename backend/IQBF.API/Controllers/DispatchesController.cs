using IQBF.Application.DTOs.Dispatches;
using IQBF.Application.Interfaces;
using IQBF.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/dispatches")]
[Authorize(Roles = "Administrator,Yard")]
public class DispatchesController : ControllerBase
{
    private readonly IDispatchService _service;
    private readonly IHubContext<OperationsHub> _hub;

    public DispatchesController(
        IDispatchService service,
        IHubContext<OperationsHub> hub)
    {
        _service = service;
        _hub = hub;
    }

    [HttpGet("shift/{shiftId:guid}")]
    public async Task<IActionResult> GetByShift(Guid shiftId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByShiftAsync(shiftId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateDispatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(
            request,
            User.Identity!.Name!,
            cancellationToken);

        await _hub.Clients.Group(OperationsHub.ShiftGroup(request.ShiftId)).SendAsync(
            "DispatchCreated",
            result,
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        CreateDispatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(
            id,
            request,
            User.Identity!.Name!,
            cancellationToken);

        await _hub.Clients.Group(OperationsHub.ShiftGroup(request.ShiftId)).SendAsync(
            "DispatchUpdated",
            result,
            cancellationToken);

        return Ok(result);
    }
}
