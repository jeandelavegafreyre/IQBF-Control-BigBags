using IQBF.Application.DTOs.Auth;
using IQBF.Application.DTOs.Users;
using IQBF.Application.Interfaces;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IQBF.Application.Services;

public class UserService : IUserService
{
    private readonly IQBFDbContext _db;
    public UserService(IQBFDbContext db) => _db = db;

    public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Login pendiente: falta integrar hashing de contraseñas y JWT.");

    public Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Registro pendiente: falta integrar hashing de contraseñas.");

    public async Task UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request, string actorUid, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(UserRole), request.Role))
            throw new ArgumentException("Rol de usuario no válido.");

        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var normalizedActorUid = (actorUid ?? string.Empty).Trim().ToUpperInvariant();

        if (user.UID == normalizedActorUid && user.Role == UserRole.Administrator && request.Role != UserRole.Administrator)
            throw new InvalidOperationException("No puedes quitarte tu propio rol de Administrador.");

        if (user.Role == UserRole.Administrator && request.Role != UserRole.Administrator && user.IsActive)
        {
            var activeAdministratorCount = await _db.Users.CountAsync(
                x => x.IsActive && x.Role == UserRole.Administrator,
                cancellationToken);

            if (activeAdministratorCount <= 1)
                throw new InvalidOperationException("No se puede cambiar el rol del último Administrador activo.");
        }

        if (user.Role == request.Role)
            return;

        user.Role = request.Role;
        user.UpdatedBy = normalizedActorUid;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
