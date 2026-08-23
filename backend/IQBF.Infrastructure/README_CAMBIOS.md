# IQBF.Infrastructure preparado

Este paquete está diseñado para trabajar con el `IQBF.Domain` corregido.

## Incluye
- `IQBFDbContext` para Entity Framework Core.
- Registro de SQL Server mediante `AddInfrastructure`.
- Configuraciones Fluent API para todas las entidades.
- Índices y restricciones de integridad.
- Cantidades `decimal(18,3)`.
- Comentarios limitados a 100 caracteres.
- Relaciones consistentes usando `Ship`, no `Vessel`.
- Timestamps UTC.

## Decisiones
1. `Reception` y `Dispatch` obtienen la nave mediante `Shift`.
2. El producto se obtiene mediante el BL; no se duplica `ProductId`.
3. La cantidad se guarda en `ReceptionItem` / `DispatchItem`.
4. BL único por `ShipId + Code`.
5. Turno único por `ShipId + ShiftDate + ShiftType`.
6. No se incluye Admin con contraseña vacía.
7. No se incluye Azure Blob todavía: el almacenamiento de fotos debe confirmarse antes.
8. No crear migraciones todavía.

## Siguiente revisión
Después de reemplazar esta carpeta, compilar la solución. Si hay errores, probablemente `IQBF.Application` todavía referencia propiedades antiguas como `VesselId`, `Quantity` en cabecera o `Dispatch.ProductId`.
