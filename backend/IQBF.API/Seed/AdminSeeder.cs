using IQBF.Domain.Entities;
using IQBF.Domain.Enums;
using IQBF.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IQBF.API.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        var uid = Normalize(configuration["SeedAdmin:UID"]);
        var password = configuration["SeedAdmin:Password"];
        var firstName = Normalize(configuration["SeedAdmin:FirstName"]);
        var lastName = Normalize(configuration["SeedAdmin:LastName"]);

        // Seed deshabilitado mientras no haya credenciales explícitas.
        if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(password))
            return;

        if (password.Length < 12)
            throw new InvalidOperationException(
                "SeedAdmin:Password debe tener al menos 12 caracteres.");

        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<IQBFDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        // No se ejecuta Migrate() aquí deliberadamente.
        // Las migraciones se controlarán de forma explícita.
        var exists = await db.Users.AnyAsync(x => x.UID == uid);
        if (exists)
            return;

        var admin = new User
        {
            UID = uid,
            FirstName = string.IsNullOrWhiteSpace(firstName) ? "ADMIN" : firstName,
            LastName = string.IsNullOrWhiteSpace(lastName) ? "IQBF" : lastName,
            Role = UserRole.Administrator,
            IsActive = true,
            CreatedBy = "SYSTEM"
        };

        admin.PasswordHash = hasher.HashPassword(admin, password);

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }

    private static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();
}
