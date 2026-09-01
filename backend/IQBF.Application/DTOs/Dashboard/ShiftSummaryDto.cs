using System.Text.Json.Serialization;
using IQBF.Domain.Enums;

namespace IQBF.Application.DTOs.Dashboard;

public record ShiftSummaryDto(
    Guid ShiftId,
    DateOnly ShiftDate,
    ShiftType ShiftType,
    ShiftStatus Status,
    DateTime StartedAt,
    DateTime? EndedAt,
    Guid ShipId,
    string ShipName,
    decimal ReceivedQuantity,
    decimal DispatchedQuantity,
    decimal NetQuantity,
    [property: JsonPropertyName("bls")]
    IReadOnlyCollection<ShiftBLBalanceDto> BLs
);