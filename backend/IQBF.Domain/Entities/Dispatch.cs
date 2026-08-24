using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

/// <summary>
/// Cabecera de un despacho.
/// La cantidad y el BL se almacenan en DispatchItem.
/// </summary>
public class Dispatch : BaseEntity
{
    public Guid ShiftId { get; set; }

    public Shift? Shift { get; set; }

    public int TransactionNumber { get; set; }

    public string Plate { get; set; } = string.Empty;

    /// <summary>
    /// Comentario operativo. La regla de máximo 100 caracteres se valida en Application/API.
    /// </summary>
    public string? Comment { get; set; }

    public ICollection<DispatchItem> Items { get; set; } = new List<DispatchItem>();

    public ICollection<DispatchPhoto> Photos { get; set; } = new List<DispatchPhoto>();
}