namespace IQBF.Application.DTOs.Reports;

public sealed record ManagementInProcessDto(
    int ShipsInProcess,
    int TotalReceived,
    int TotalDispatched,
    int TotalAvailable,
    IReadOnlyList<ManagementInProcessShipDto> Ships);

public sealed record ManagementInProcessShipDto(
    Guid ShipId,
    string ShipName,
    DateTime FirstReceptionAt,
    DateTime? LastDispatchAt,
    DateTime LastMovementAt,
    int DeclaredQuantity,
    int ReceivedQuantity,
    int DispatchedQuantity,
    int AvailableQuantity,
    double DispatchProgress,
    double CalendarDurationHours,
    int BlCount,
    int ReceptionTransactions,
    int DispatchTransactions,
    IReadOnlyList<string> Products);
