namespace IQBF.Application.DTOs.Dashboard;

public record ShiftBLBalanceDto(
    Guid BLId,
    string BLCode,
    string ProductName,
    int ReceivedQuantity,
    int DispatchedQuantity,
    int NetQuantity
);