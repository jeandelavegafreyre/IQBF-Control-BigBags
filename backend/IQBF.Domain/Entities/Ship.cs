using IQBF.Domain.Common;
using IQBF.Domain.Enums;

namespace IQBF.Domain.Entities;

/// <summary>
/// Nave asociada a BLs y turnos operativos.
/// </summary>
public class Ship : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ShipStatus Status { get; set; } = ShipStatus.Active;

    public ICollection<BL> BLs { get; set; } = new List<BL>();

    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
