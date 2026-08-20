using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class Product : BaseEntity
{
    /// <summary>
    /// Nombre del producto.
    /// Ejemplo:
    /// CARBONATO DE SODIO
    /// SULFATO DE SODIO
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el producto está disponible para uso.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Relación con los BL asociados al producto.
    /// </summary>
    public ICollection<BL> BLs { get; set; } = new List<BL>();
}
