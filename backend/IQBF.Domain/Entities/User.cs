using IQBF.Domain.Common;
using IQBF.Domain.Enums;

namespace IQBF.Domain.Entities;

/// <summary>
/// Usuario de Control IQBF.
/// El UID, nombre y apellido deben normalizarse a mayúsculas en la capa de aplicación.
/// </summary>
public class User : BaseEntity
{
    public string UID { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Hash de contraseña. Nunca almacenar contraseñas en texto plano.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Todo usuario nuevo inicia como User hasta que un administrador cambie su rol.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
