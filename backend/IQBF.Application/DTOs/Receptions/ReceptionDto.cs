namespace IQBF.Application.DTOs.Receptions;

public sealed record ReceptionItemDto(
    Guid BLId,
    string BLCode,
    decimal Quantity
);

public sealed record ReceptionDto(
    Guid Id,
    Guid ShiftId,
    int TransactionNumber,
    string TerminalTruck,
    string? Comment,
    DateTime CreatedAt,
    string? CreatedBy,
    IReadOnlyCollection<ReceptionItemDto> Items
);
