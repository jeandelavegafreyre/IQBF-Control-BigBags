namespace IQBF.Application.DTOs.Reports;

public sealed record ManagementReportDto(
    int Year,
    int Month,
    int CompletedShips,
    int TotalReceived,
    int TotalDispatched,
    int TotalDeclared,
    IReadOnlyList<ManagementShipDto> Ships,
    IReadOnlyList<ManagementMonthlyTrendDto> MonthlyTrend,
    IReadOnlyList<ManagementProductDto> Products);

public sealed record ManagementShipDto(
    Guid ShipId,
    string ShipName,
    DateTime? FirstReceptionAt,
    DateTime LastDispatchAt,
    int DeclaredQuantity,
    int ReceivedQuantity,
    int DispatchedQuantity,
    int AvailableQuantity,
    int ReceptionTransactions,
    int DispatchTransactions,
    double CalendarDurationHours,
    int BlCount,
    IReadOnlyList<string> Products);

public sealed record ManagementMonthlyTrendDto(
    int Month,
    int CompletedShips,
    int DispatchedQuantity);

public sealed record ManagementProductDto(
    string ProductName,
    int DeclaredQuantity,
    int ReceivedQuantity,
    int DispatchedQuantity,
    int ShipCount);
