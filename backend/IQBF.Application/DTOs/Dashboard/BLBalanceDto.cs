namespace IQBF.Application.DTOs.Dashboard;

public record BLBalanceDto(
    Guid Id,
    string Code,
    string ProductName,
    int TotalQuantity,
    int ReceivedQuantity,
    int DispatchedQuantity,
    int AvailableQuantity,
    int PendingReception,
    decimal ReceptionProgress,
    decimal DispatchProgress
);
