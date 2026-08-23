using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

/// <summary>
/// Evidencia fotográfica asociada a un despacho.
/// El límite de tres fotos se valida en la capa de aplicación.
/// </summary>
public class DispatchPhoto : BaseEntity
{
    public Guid DispatchId { get; set; }

    /// <summary>
    /// URL o ruta del archivo en el almacenamiento autorizado.
    /// </summary>
    public string PhotoUrl { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public Dispatch? Dispatch { get; set; }
}
