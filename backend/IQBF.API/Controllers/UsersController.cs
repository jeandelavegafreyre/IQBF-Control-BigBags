using IQBF.API.Security;
using IQBF.Application.DTOs.Users;
using IQBF.Application.Interfaces;
using IQBF.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IQBF.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    private readonly IAuthService _authService;
    private readonly IQBFDbContext _db;

    public UsersController(IUserService service, IAuthService authService, IQBFDbContext db)
    {
        _service = service;
        _authService = authService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _db.Users.AsNoTracking().OrderBy(x => x.UID)
            .Select(x => new UserDto(x.Id, x.UID, x.FirstName, x.LastName, (x.FirstName + " " + x.LastName).Trim(), x.Role, x.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateRoleAsync(userId, request, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpPut("{userId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        await _service.UpdateStatusAsync(userId, request, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }

    [HttpPut("{userId:guid}/password")]
    public async Task<IActionResult> ResetPassword(Guid userId, ResetUserPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ResetPasswordAsync(userId, request, User.Identity!.Name!, cancellationToken);
        return NoContent();
    }
}
