using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class DispatchItem : BaseEntity
{
    /// <summary>Cantidad despachada en Big Bags completos.</summary>
    public int Quantity { get; set; }
    public Guid DispatchId { get; set; }
    public Dispatch? Dispatch { get; set; }
    public Guid BLId { get; set; }
    public BL? BL { get; set; }
}
