using IQBF.Application.DTOs.Auth;
using IQBF.Application.DTOs.Users;
using IQBF.Application.Interfaces;
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
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.Role = request.Role;
        user.UpdatedBy = actorUid;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
