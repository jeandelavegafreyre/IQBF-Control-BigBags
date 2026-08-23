using IQBF.Domain.Enums;
namespace IQBF.Application.DTOs.Ships;
public sealed record ShipDto(Guid Id, string Name, ShipStatus Status);
