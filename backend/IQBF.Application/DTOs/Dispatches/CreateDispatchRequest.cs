namespace IQBF.Application.DTOs.Dispatches;
public sealed record CreateDispatchRequest(Guid ShiftId, string Plate, string? Comment, IReadOnlyCollection<DispatchItemRequest> Items);
