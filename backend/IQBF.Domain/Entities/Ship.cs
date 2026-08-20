using IQBF.Domain.Common;
using IQBF.Domain.Enums;

namespace IQBF.Domain.Entities;

public class Ship : BaseEntity
{
    /// <summary>
    /// Nombre de la nave.
    /// Ejemplo: CL HEIDI, YIN CAI, LINDEN ARROW.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Estado operativo de la nave.
    /// Solo las naves activas podrán seleccionarse al iniciar turno.
    /// </summary>
    public ShipStatus Status { get; set; } = ShipStatus.Active;

    /// <summary>
    /// Relación con los BL asociados a la nave.
    /// </summary>
    public ICollection<BL> BLs { get; set; } = new List<BL>();

    /// <summary>
    /// Relación con los turnos operados en la nave.
    /// </summary>
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
