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
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "La sesión no es válida.",
                    status = StatusCodes.Status401Unauthorized
                });
                return;
            }

            var user = await db.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => new { x.IsActive, x.Role })
                .FirstOrDefaultAsync(context.RequestAborted);

            if (user is null || !user.IsActive)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "El usuario está inactivo o ya no existe.",
                    status = StatusCodes.Status401Unauthorized
                });
                return;
            }

            var tokenRole = context.User.FindFirstValue(ClaimTypes.Role);
            if (!string.Equals(tokenRole, user.Role.ToString(), StringComparison.Ordinal))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Los permisos de la sesión cambiaron. Inicia sesión nuevamente.",
                    status = StatusCodes.Status401Unauthorized
                });
                return;
            }
        }

        await _next(context);
    }
}
