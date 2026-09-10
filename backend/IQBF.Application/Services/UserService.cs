using System.Data;
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
    private readonly IUserSessionRevoker _sessionRevoker;

    public UserService(IQBFDbContext db, IUserSessionRevoker sessionRevoker)
    {
        _db = db;
        _sessionRevoker = sessionRevoker;
    }

    public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Login pendiente: falta integrar hashing de contraseñas y JWT.");

    public Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Registro pendiente: falta integrar hashing de contraseñas.");

    public async Task UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request, string actorUid, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(UserRole), request.Role))
            throw new ArgumentException("Rol de usuario no válido.");

        var normalizedActorUid = (actorUid ?? string.Empty).Trim().ToUpperInvariant();
        var strategy = _db.Database.CreateExecutionStrategy();
        var changed = false;

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");

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

            changed = user.Role != request.Role;
            if (changed)
            {
                user.Role = request.Role;
                user.SecurityVersion++;
                user.UpdatedBy = normalizedActorUid;
                await _db.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });

        if (changed)
            _sessionRevoker.Revoke(userId);
    }

    public async Task UpdateStatusAsync(Guid userId, UpdateUserStatusRequest request, string actorUid, CancellationToken cancellationToken = default)
    {
        var normalizedActorUid = (actorUid ?? string.Empty).Trim().ToUpperInvariant();
        var strategy = _db.Database.CreateExecutionStrategy();
        var changed = false;

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
                ?? throw new KeyNotFoundException("Usuario no encontrado.");

            changed = user.IsActive != request.IsActive;
            if (changed)
            {
                if (user.UID == normalizedActorUid && !request.IsActive)
                    throw new InvalidOperationException("No puedes desactivar tu propia cuenta durante una sesión activa.");

                if (user.Role == UserRole.Administrator && user.IsActive && !request.IsActive)
                {
                    var activeAdministratorCount = await _db.Users.CountAsync(
                        x => x.IsActive && x.Role == UserRole.Administrator,
                        cancellationToken);

                    if (activeAdministratorCount <= 1)
                        throw new InvalidOperationException("No se puede desactivar al último Administrador activo.");
                }

                user.IsActive = request.IsActive;
                user.SecurityVersion++;
                user.UpdatedBy = normalizedActorUid;
                await _db.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });

        if (changed)
            _sessionRevoker.Revoke(userId);
    }
}
