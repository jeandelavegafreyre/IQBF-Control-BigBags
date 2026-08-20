namespace IQBF.Domain.Common;

public abstract class BaseEntity
{
    /// <summary>
    /// Clave primaria universal.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Fecha y hora de creación (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// UID o identificador del usuario que creó el registro.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Fecha y hora de última actualización (UTC).
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// UID o identificador del usuario que realizó la última modificación.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
