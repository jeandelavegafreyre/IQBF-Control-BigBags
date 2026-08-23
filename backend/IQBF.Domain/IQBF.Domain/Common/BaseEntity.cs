namespace IQBF.Domain.Common;

/// <summary>
/// Entidad base con identificador y campos de auditoría.
/// Las fechas se almacenan en UTC y se convierten a hora local en la capa de presentación/reportes.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Clave primaria universal.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Fecha y hora de creación en UTC.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UID del usuario que creó el registro.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización en UTC.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// UID del usuario que realizó la última modificación.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
