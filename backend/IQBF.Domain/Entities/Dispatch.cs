using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class Dispatch : BaseEntity
{
    public Guid ShiftId { get; set; }

    public Guid VesselId { get; set; }

    public string Plate { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public Guid? ProductId { get; set; }

    public string? Comment { get; set; }

    public string OperatorName { get; set; } = string.Empty;

    // Navigation Properties

    public Shift? Shift { get; set; }

    public Vessel? Vessel { get; set; }

    public Product? Product { get; set; }

    public ICollection<DispatchPhoto> Photos { get; set; }
        = new List<DispatchPhoto>();
}
