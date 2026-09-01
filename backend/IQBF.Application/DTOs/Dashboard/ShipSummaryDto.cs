namespace IQBF.Application.DTOs.Dashboard;

public record ShipSummaryDto(
    Guid ShipId,
    string ShipName,
    decimal TotalQuantity,
    decimal ReceivedQuantity,
    decimal DispatchedQuantity,
    decimal AvailableQuantity,
    decimal PendingReception,
    decimal ReceptionProgress,
    decimal DispatchProgress,
    IReadOnlyCollection<BLBalanceDto> BLs
);
