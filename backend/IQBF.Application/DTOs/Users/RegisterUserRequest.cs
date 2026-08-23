namespace IQBF.Application.DTOs.Users;
public sealed record RegisterUserRequest(string UID, string FirstName, string LastName, string Password);
