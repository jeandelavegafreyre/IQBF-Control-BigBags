using IQBF.Application.DTOs.BLs;
using IQBF.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/bls")]
[Authorize]
public class BLsController : ControllerBase
{
    private readonly IBLService _service;
    public BLsController(IBLService service) => _service = service;

    [HttpGet("by-ship/{shipId:guid}")]
    public async Task<IActionResult> GetByShip(Guid shipId, CancellationToken cancellationToken) =>
        Ok(await _service.GetByShipAsync(shipId, cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreateBLRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateAsync(request, User.Identity!.Name!, cancellationToken));
}
