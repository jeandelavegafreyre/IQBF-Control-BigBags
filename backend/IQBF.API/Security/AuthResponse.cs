using IQBF.Domain.Enums;

namespace IQBF.API.Security;

public sealed record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    Guid UserId,
    string UID,
    string FullName,
    UserRole Role
);
