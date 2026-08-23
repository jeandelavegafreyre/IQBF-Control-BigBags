namespace IQBF.Application.DTOs.Receptions;
public sealed record CreateReceptionRequest(Guid ShiftId, string TerminalTruck, string? Comment, IReadOnlyCollection<ReceptionItemRequest> Items);
