using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class DispatchPhoto : BaseEntity
{
    public Guid DispatchId { get; set; }

    public string PhotoUrl { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    // Navigation Property
    public Dispatch Dispatch { get; set; } = null!;
}
