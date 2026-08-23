using IQBF.Domain.Enums;
namespace IQBF.Application.DTOs.Users;
public sealed record UserDto(Guid Id, string UID, string FirstName, string LastName, string FullName, UserRole Role, bool IsActive);
