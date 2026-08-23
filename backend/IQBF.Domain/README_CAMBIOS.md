# IQBF.Domain corregido

Esta carpeta reemplaza la versión revisada del commit:

`63a114a078ddc5be80c2e22525afbff1528481c7`

## Decisiones fijadas

- Se usa `Ship` / `ShipId` en todo el dominio. Se elimina el concepto inconsistente `Vessel`.
- `Reception` y `Dispatch` pertenecen a un `Shift`; la nave se obtiene mediante `Shift.Ship`.
- Las cantidades se almacenan solamente en `ReceptionItem` y `DispatchItem`.
- `DispatchItem` deja de estar vacío y queda simétrico con `ReceptionItem`.
- `ProductId` no se repite en `Dispatch`: el producto se obtiene mediante `BL.Product`.
- `OperatorName` se elimina de las transacciones; la trazabilidad usa `CreatedBy` y `UpdatedBy`.
- Los roles definitivos son `Administrator`, `Yard` y `User`.
- Los usuarios nuevos tienen rol `User` por defecto.
- Fechas de auditoría se almacenan en UTC.
- Las reglas de máximo 3 fotos, comentario de 100 caracteres, mayúsculas y validaciones de negocio se implementarán en Application/API.
- Se incluye `IQBF.Domain.csproj` para .NET 8.

## Importante

Todavía no crear migraciones de Entity Framework con este dominio hasta revisar y ajustar `IQBF.Infrastructure`.
