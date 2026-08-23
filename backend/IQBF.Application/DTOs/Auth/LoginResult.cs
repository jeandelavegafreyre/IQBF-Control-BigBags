using IQBF.Domain.Enums;
namespace IQBF.Application.DTOs.Auth;
public sealed record LoginResult(Guid UserId, string UID, string FullName, UserRole Role);
