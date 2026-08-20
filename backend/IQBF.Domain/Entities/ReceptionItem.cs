using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class ReceptionItem : BaseEntity
{
    /// <summary>
    /// Cantidad recepcionada para el BL.
    /// </summary>
    public decimal Quantity { get; set; }

    // =====================================================
    // RELACIONES
    // =====================================================

    /// <summary>
    /// Recepción a la que pertenece el detalle.
    /// </summary>
    public Guid ReceptionId { get; set; }

    public Reception? Reception { get; set; }

    /// <summary>
    /// BL asociado al movimiento.
    /// </summary>
    public Guid BLId { get; set; }

    public BL? BL { get; set; }
}
