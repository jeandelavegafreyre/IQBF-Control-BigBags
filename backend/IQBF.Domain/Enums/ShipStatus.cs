namespace IQBF.Domain.Enums;

/// <summary>
/// Estado operativo de una nave.
/// Solo las naves activas pueden seleccionarse al iniciar un turno.
/// </summary>
public enum ShipStatus
{
    Active = 1,
    Inactive = 2
}
