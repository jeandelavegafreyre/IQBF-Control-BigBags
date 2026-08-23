namespace IQBF.Application.DTOs.Dispatches;
public sealed record DispatchItemDto(Guid BLId, string BLCode, decimal Quantity);
public sealed record DispatchDto(Guid Id, Guid ShiftId, string Plate, string? Comment, DateTime CreatedAt, string? CreatedBy, IReadOnlyCollection<DispatchItemDto> Items);
