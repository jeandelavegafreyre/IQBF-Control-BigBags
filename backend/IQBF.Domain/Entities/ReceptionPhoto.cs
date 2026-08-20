using IQBF.Domain.Common;

namespace IQBF.Domain.Entities;

public class ReceptionPhoto : BaseEntity
{
    public Guid ReceptionId { get; set; }

    public string PhotoUrl { get; set; } = string.Empty;

    public string? FileName { get; set; }

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    // Navigation Property
    public Reception Reception { get; set; } = null!;
}
