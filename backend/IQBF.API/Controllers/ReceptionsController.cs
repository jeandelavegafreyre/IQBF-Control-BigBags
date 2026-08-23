using IQBF.Application.DTOs.Receptions;
using IQBF.Application.Interfaces;
using IQBF.API.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/receptions")]
[Authorize(Roles = "Administrator,Yard")]
public class ReceptionsController : ControllerBase
{
    private readonly IReceptionService _service;
    private readonly IHubContext<OperationsHub> _hub;

    public ReceptionsController(
        IReceptionService service,
        IHubContext<OperationsHub> hub)
    {
        _service = service;
        _hub = hub;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateReceptionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(
            request,
            User.Identity!.Name!,
            cancellationToken);

        await _hub.Clients.All.SendAsync(
            "ReceptionCreated",
            result,
            cancellationToken);

        return Ok(result);
    }
}
