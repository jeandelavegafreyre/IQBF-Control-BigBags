using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class Reception : BaseEntity
{
    public Guid ShiftId { get; set; }

    public Guid VesselId { get; set; }

    public string TerminalTruck { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string? Comment { get; set; }

    public string OperatorName { get; set; } = string.Empty;

    // Navigation Properties
    public ICollection<ReceptionItem> Items { get; set; }
        = new List<ReceptionItem>();

    public ICollection<ReceptionPhoto> Photos { get; set; }
        = new List<ReceptionPhoto>();
}
