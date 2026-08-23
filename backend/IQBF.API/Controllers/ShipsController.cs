using IQBF.Application.DTOs.Ships;
using IQBF.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/ships")]
[Authorize]
public class ShipsController : ControllerBase
{
    private readonly IShipService _service;
    public ShipsController(IShipService service) => _service = service;

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken) =>
        Ok(await _service.GetActiveAsync(cancellationToken));

    [HttpPost]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Create(CreateShipRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CreateAsync(request, User.Identity!.Name!, cancellationToken));
}
