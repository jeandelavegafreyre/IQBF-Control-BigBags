using IQBF.Application.DTOs.Auth;
using IQBF.Application.DTOs.Users;

namespace IQBF.API.Security;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
}
