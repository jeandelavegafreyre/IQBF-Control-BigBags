using IQBF.Domain.Common;
using IQBF.Domain.Enums;

namespace IQBF.Domain.Entities;

/// <summary>
/// Turno operativo de una nave para una fecha y tipo de turno.
/// Las recepciones y despachos se vinculan a este turno.
/// </summary>
public class Shift : BaseEntity
{
    public DateOnly ShiftDate { get; set; }

    public ShiftType ShiftType { get; set; }

    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    /// <summary>
    /// Hora real de apertura, almacenada en UTC.
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Hora real de cierre, almacenada en UTC.
    /// </summary>
    public DateTime? EndedAt { get; set; }

    public Guid ShipId { get; set; }

    public Ship? Ship { get; set; }

    public ICollection<Reception> Receptions { get; set; } = new List<Reception>();

    public ICollection<Dispatch> Dispatches { get; set; } = new List<Dispatch>();
}
