using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class Reception : BaseEntity
{
    /// <summary>
    /// Número de Terminal Truck.
    /// Solo valores numéricos según la regla de negocio.
    /// </summary>
    public string TruckNumber { get; set; } = string.Empty;

    /// <summary>
    /// Comentario opcional.
    /// Máximo 100 caracteres.
    /// </summary>
    public string? Comments { get; set; }

    // =====================================================
    // RELACIONES
    // =====================================================

    /// <summary>
    /// Turno al que pertenece la recepción.
    /// </summary>
    public Guid ShiftId { get; set; }

    public Shift? Shift { get; set; }

    /// <summary>
    /// Usuario que registró la recepción.
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Detalle de BLs asociados a la recepción.
    /// Permite múltiples BL por transacción.
    /// </summary>
    public ICollection<ReceptionItem> ReceptionItems { get; set; }
     
