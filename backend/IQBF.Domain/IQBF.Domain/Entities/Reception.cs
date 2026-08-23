using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

/// <summary>
/// Cabecera de una recepción.
/// Una recepción puede contener uno o varios BL mediante ReceptionItem.
/// </summary>
public class Reception : BaseEntity
{
    public Guid ShiftId { get; set; }

    public Shift? Shift { get; set; }

    /// <summary>
    /// Número/identificador del Terminal Truck.
    /// La validación de entrada numérica se aplica en Application/API.
    /// </summary>
    public string TerminalTruck { get; set; } = string.Empty;

    /// <summary>
    /// Comentario operativo. La regla de máximo 100 caracteres se valida en Application/API.
    /// </summary>
    public string? Comment { get; set; }

    public ICollection<ReceptionItem> Items { get; set; } = new List<ReceptionItem>();

    public ICollection<ReceptionPhoto> Photos { get; set; } = new List<ReceptionPhoto>();
}
