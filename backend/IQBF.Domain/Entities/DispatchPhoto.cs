using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class DispatchPhoto : BaseEntity
{
    /// <summary>
    /// URL de la evidencia fotográfica almacenada.
    /// Puede apuntar a Azure Blob Storage o al repositorio corporativo definido.
    /// </summary>
    public string PhotoUrl { get; set; } = string.Empty;

    // =====================================================
    // RELACIONES
    // =====================================================

    /// <summary>
    /// Despacho al que pertenece la fotografía.
    /// </summary>
    public Guid DispatchId { get; set; }

    public Dispatch? Dispatch { get; set; }
}
