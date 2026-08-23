using IQBF.Application.DTOs.Auth;
using IQBF.Application.DTOs.Users;
namespace IQBF.Application.Interfaces;
public interface IUserService
{
    Task<UserDto> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(Guid userId, UpdateUserRoleRequest request, string actorUid, CancellationToken cancellationToken = default);
}
