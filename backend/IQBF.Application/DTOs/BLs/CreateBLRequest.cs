namespace IQBF.Application.DTOs.BLs;
public sealed record CreateBLRequest(string Code, decimal TotalQuantity, Guid ShipId, Guid ProductId);
