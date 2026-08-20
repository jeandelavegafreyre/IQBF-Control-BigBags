using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class BL : BaseEntity
{
    /// <summary>
    /// Código único del Bill of Lading.
    /// Ejemplo: 20CL201CS006
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad total declarada para el lote.
    /// </summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>
    /// Estado del BL.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // =====================================================
    // RELACIONES
    // =====================================================

    /// <summary>
    /// Nave a la que pertenece el BL.
    /// </summary>
    public Guid ShipId { get; set; }

    public Ship? Ship { get; set; }

    /// <summary>
    /// Producto asociado al BL.
    /// </summary>
    public Guid ProductId { get; set; }

    public Product? Product { get; set; }

    /// <summary>
    /// Recepciones asociadas al BL.
    /// </summary>
    public ICollection<ReceptionItem> ReceptionItems { get; set; }
        = new List<ReceptionItem>();

    /// <summary>
    /// Despachos asociados al BL.
    /// </summary>
    public ICollection<DispatchItem> DispatchItems { get; set; }
        = new List<DispatchItem>();
}
