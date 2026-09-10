using System.Security.Claims;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IQBF.API.Hubs;

[Authorize(Roles = "Administrator,Yard")]
public class OperationsHub : Hub
{
    private readonly IQBFDbContext _db;

    public OperationsHub(IQBFDbContext db)
    {
        _db = db;
    }

    public static string ShiftGroup(Guid shiftId) => $"shift:{shiftId:N}";

    public async Task JoinShift(Guid shiftId)
    {
        if (!await IsCurrentSessionValidAsync())
            throw new HubException("La sesión cambió o dejó de ser válida.");

        var shiftIsOpen = await _db.Shifts
            .AsNoTracking()
            .AnyAsync(
                x => x.Id == shiftId && x.Status == ShiftStatus.Open,
                Context.ConnectionAborted);

        if (!shiftIsOpen)
            throw new HubException("El turno no está abierto o ya no existe.");

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            ShiftGroup(shiftId),
            Context.ConnectionAborted);
    }

    public Task<bool> ValidateSession() => IsCurrentSessionValidAsync();

    private async Task<bool> IsCurrentSessionValidAsync()
    {
        var userIdValue = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleValue = Context.User?.FindFirstValue(ClaimTypes.Role);
        var securityVersionValue = Context.User?.FindFirstValue("security_version");

        if (!Guid.TryParse(userIdValue, out var userId) ||
            !int.TryParse(securityVersionValue, out var tokenSecurityVersion))
            return false;

        var user = await _db.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .Select(x => new { x.IsActive, x.Role, x.SecurityVersion })
            .FirstOrDefaultAsync(Context.ConnectionAborted);

        return user is not null &&
               user.IsActive &&
               user.SecurityVersion == tokenSecurityVersion &&
               string.Equals(roleValue, user.Role.ToString(), StringComparison.Ordinal) &&
               (user.Role == UserRole.Administrator || user.Role == UserRole.Yard);
    }
}
