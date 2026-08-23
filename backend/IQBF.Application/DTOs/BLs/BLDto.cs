namespace IQBF.Application.DTOs.BLs;
public sealed record BLDto(Guid Id, string Code, decimal TotalQuantity, bool IsActive, Guid ShipId, string ShipName, Guid ProductId, string ProductName);
