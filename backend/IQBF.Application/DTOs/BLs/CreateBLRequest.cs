namespace IQBF.Application.DTOs.BLs;
public sealed record CreateBLRequest(string Code, int TotalQuantity, Guid ShipId, Guid ProductId);
