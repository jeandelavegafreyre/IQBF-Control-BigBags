# IQBF.Application preparado

Compatible con los paquetes corregidos de `IQBF.Domain` e `IQBF.Infrastructure`.

## Incluye
- DTOs.
- Interfaces de servicios.
- Servicios para Ship, Product, BL, Shift, Reception, Dispatch y User.
- `DependencyInjection.cs`.

## Reglas implementadas
- Solo naves activas pueden iniciar turno.
- BL asociado a nave y producto válidos.
- Recepción/Despacho solo en turnos abiertos.
- BL de una operación debe pertenecer a la nave del turno.
- Cantidades > 0.
- Comentarios <= 100 caracteres.
- Terminal Truck solo numérico.
- Placa, nombres y códigos maestros normalizados a mayúsculas.
- Sin BL duplicado dentro de una misma operación.

## Pendiente deliberadamente
- Fotos: falta definir almacenamiento corporativo.
- Hashing de contraseñas.
- JWT y autorización HTTP.
- SignalR.
- Reportes.

## Importante
No crear migraciones ni ejecutar `database update` todavía.
