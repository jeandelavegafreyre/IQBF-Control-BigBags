using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class ReceptionItem : BaseEntity
{
    /// <summary>Cantidad recibida en Big Bags completos.</summary>
    public int Quantity { get; set; }
    public Guid ReceptionId { get; set; }
    public Reception? Reception { get; set; }
    public Guid BLId { get; set; }
    public BL? BL { get; set; }
}
