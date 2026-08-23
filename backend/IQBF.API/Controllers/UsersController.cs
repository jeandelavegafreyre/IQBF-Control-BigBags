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
    private readonly IQBFDbContext _db;

    public UsersController(IUserService service, IQBFDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _db.Users
            .AsNoTracking()
            .OrderBy(x => x.UID)
            .Select(x => new UserDto(
                x.Id,
                x.UID,
                x.FirstName,
                x.LastName,
                (x.FirstName + " " + x.LastName).Trim(),
                x.Role,
                x.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<IActionResult> UpdateRole(
        Guid userId,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _service.UpdateRoleAsync(
            userId,
            request,
            User.Identity!.Name!,
            cancellationToken);

        return NoContent();
    }
}
