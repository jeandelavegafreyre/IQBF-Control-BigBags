using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

/// <summary>
/// Producto del catálogo maestro.
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<BL> BLs { get; set; } = new List<BL>();
}
