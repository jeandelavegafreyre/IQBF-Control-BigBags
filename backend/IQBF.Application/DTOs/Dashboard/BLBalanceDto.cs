namespace IQBF.Application.DTOs.Dashboard;

public record BLBalanceDto(
    Guid Id,
    string Code,
    string ProductName,
    decimal TotalQuantity,
    decimal ReceivedQuantity,
    decimal DispatchedQuantity,
    decimal AvailableQuantity,
    decimal PendingReception,
    decimal ReceptionProgress,
    decimal DispatchProgress
);
