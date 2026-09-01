namespace IQBF.Application.DTOs.Dashboard;

public record ShiftBLBalanceDto(
    Guid BLId,
    string BLCode,
    string ProductName,
    decimal ReceivedQuantity,
    decimal DispatchedQuantity,
    decimal NetQuantity
);