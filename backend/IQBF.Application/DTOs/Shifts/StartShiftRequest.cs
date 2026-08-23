using IQBF.Domain.Enums;
namespace IQBF.Application.DTOs.Shifts;
public sealed record StartShiftRequest(Guid ShipId, DateOnly ShiftDate, ShiftType ShiftType);
