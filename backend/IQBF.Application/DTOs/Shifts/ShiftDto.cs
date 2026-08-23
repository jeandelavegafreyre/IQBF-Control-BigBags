using IQBF.Domain.Enums;
namespace IQBF.Application.DTOs.Shifts;
public sealed record ShiftDto(Guid Id, DateOnly ShiftDate, ShiftType ShiftType, ShiftStatus Status, DateTime StartedAt, DateTime? EndedAt, Guid ShipId, string ShipName);
