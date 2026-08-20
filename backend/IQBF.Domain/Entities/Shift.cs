using IQBF.Domain.Common;
using IQBF.Domain.Enums;

namespace IQBF.Domain.Entities;

public class Shift : BaseEntity
{
    /// <summary>
    /// Fecha operativa del turno.
    /// </summary>
    public DateOnly ShiftDate { get; set; }

    /// <summary>
    /// Tipo de turno.
    /// Day = 06:00 - 18:00
    /// Night = 18:00 - 06:00
    /// </summary>
    public ShiftType ShiftType { get; set; }

    /// <summary>
    /// Estado del turno.
    /// Open / Closed
    /// </summary>
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;

    /// <summary>
    /// Fecha y hora de apertura.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Fecha y hora de cierre.
    /// </summary>
    public DateTime? EndedAt { get; set; }

    // =====================================================
    // RELACIONES
    // =====================================================

    /// <summary>
    /// Nave seleccionada al iniciar el turno.
    /// </summary>
    public Guid ShipId { get; set; }

    public Ship? Ship { get; set; }

    /// <summary>
    /// 
