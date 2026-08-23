using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

/// <summary>
/// Detalle de un despacho para un BL específico.
/// Mantiene el modelo simétrico con ReceptionItem.
/// </summary>
public class DispatchItem : BaseEntity
{
    /// <summary>
    /// Cantidad despachada en Big Bags para el BL.
    /// </summary>
    public decimal Quantity { get; set; }

    public Guid DispatchId { get; set; }

    public Dispatch? Dispatch { get; set; }

    public Guid BLId { get; set; }

    public BL? BL { get; set; }
}
