using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

/// <summary>
/// Bill of Lading asociado estrictamente a una nave y a un producto.
/// </summary>
public class BL : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    /// <summary>Cantidad total declarada en Big Bags completos.</summary>
    public int TotalQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid ShipId { get; set; }
    public Ship? Ship { get; set; }
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public ICollection<ReceptionItem> ReceptionItems { get; set; } = new List<ReceptionItem>();
    public ICollection<DispatchItem> DispatchItems { get; set; } = new List<DispatchItem>();
}
