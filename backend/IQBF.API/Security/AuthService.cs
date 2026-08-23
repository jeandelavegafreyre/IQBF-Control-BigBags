using IQBF.Application.DTOs.Auth;
using IQBF.Application.DTOs.Users;
using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IQBF.API.Security;

public class AuthService : IAuthService
{
    private readonly IQBFDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        IQBFDbContext db,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var uid = Normalize(request.UID);

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.UID == uid, cancellationToken)
            ?? throw new UnauthorizedAccessException("UID o contraseña incorrectos.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("El usuario está inactivo.");

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("UID o contraseña incorrectos.");

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.UpdatedBy = user.UID;
            await _db.SaveChangesAsync(cancellationToken);
        }

        return CreateResponse(user);
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var uid = Normalize(request.UID);
        var firstName = Normalize(request.FirstName);
        var lastName = Normalize(request.LastName);

        if (string.IsNullOrWhiteSpace(uid))
            throw new ArgumentException("UID es obligatorio.");

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Apellido es obligatorio.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("La contraseña debe tener al menos 8 caracteres.");

        if (await _db.Users.AnyAsync(x => x.UID == uid, cancellationToken))
            throw new InvalidOperationException("El UID ya está registrado.");

        var user = new User
        {
            UID = uid,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.User,
            IsActive = true,
            CreatedBy = uid
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return CreateResponse(user);
    }

    private AuthResponse CreateResponse(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key no configurado.");

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var parsed)
            ? parsed
            : 480;

        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new[]
        {
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, user.UID),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, user.Role.ToString()),
            new System.Security.Claims.Claim("uid", user.UID),
            new System.Security.Claims.Claim("full_name", user.FullName)
        };

        var credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(key)),
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenValue = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler()
            .WriteToken(token);

        return new AuthResponse(
            tokenValue,
            expiresAt,
            user.Id,
            user.UID,
            user.FullName,
            user.Role);
    }

    private static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();
}
