using System.Security.Claims;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.API.Middleware;

public sealed class ActiveUserMiddleware
{
    private readonly RequestDelegate _next;

    public ActiveUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IQBFDbContext db)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                await RejectAsync(context, "La sesión no es válida.");
                return;
            }

            var user = await db.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new { x.IsActive, x.Role, x.SecurityVersion })
                .FirstOrDefaultAsync(context.RequestAborted);

            if (user is null || !user.IsActive)
            {
                await RejectAsync(context, "El usuario está inactivo o ya no existe.");
                return;
            }

            var tokenRole = context.User.FindFirstValue(ClaimTypes.Role);
            if (!string.Equals(tokenRole, user.Role.ToString(), StringComparison.Ordinal))
            {
                await RejectAsync(context, "Los permisos de la sesión cambiaron. Inicia sesión nuevamente.");
                return;
            }

            var securityVersionValue = context.User.FindFirstValue("security_version");
            if (!int.TryParse(securityVersionValue, out var tokenSecurityVersion) ||
                tokenSecurityVersion != user.SecurityVersion)
            {
                await RejectAsync(context, "La seguridad de la cuenta cambió. Inicia sesión nuevamente.");
                return;
            }
        }

        await _next(context);
    }

    private static async Task RejectAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsJsonAsync(new
        {
            error = message,
            status = StatusCodes.Status401Unauthorized
        });
    }
}
