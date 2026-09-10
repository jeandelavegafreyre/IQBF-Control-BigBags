namespace IQBF.Application.DTOs.Dashboard;

public record ShipSummaryDto(
    Guid ShipId,
    string ShipName,
    int TotalQuantity,
    int ReceivedQuantity,
    int DispatchedQuantity,
    int AvailableQuantity,
    int PendingReception,
    decimal ReceptionProgress,
    decimal DispatchProgress,
    IReadOnlyCollection<BLBalanceDto> BLs
);
