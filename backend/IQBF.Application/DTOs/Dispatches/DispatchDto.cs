namespace IQBF.Application.DTOs.Dispatches;

public sealed record DispatchItemDto(Guid BLId, string BLCode, int Quantity);

public sealed record DispatchDto(
    Guid Id,
    Guid ShiftId,
    int TransactionNumber,
    string Plate,
    string? Comment,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    IReadOnlyCollection<DispatchItemDto> Items
);
