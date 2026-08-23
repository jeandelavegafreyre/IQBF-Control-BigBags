using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

/// <summary>
/// Detalle de una recepción para un BL específico.
/// </summary>
public class ReceptionItem : BaseEntity
{
    /// <summary>
    /// Cantidad recibida en Big Bags para el BL.
    /// </summary>
    public decimal Quantity { get; set; }

    public Guid ReceptionId { get; set; }

    public Reception? Reception { get; set; }

    public Guid BLId { get; set; }

    public BL? BL { get; set; }
}
