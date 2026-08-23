# IQBF.API preparado

Compatible con:
- IQBF.Domain corregido
- IQBF.Infrastructure corregido
- IQBF.Application corregido

## Incluye
- Program.cs completo
- JWT Authentication
- Password hashing con ASP.NET Core Identity PasswordHasher
- Registro/Login
- Roles Administrator / Yard / User
- CORS
- Swagger
- Middleware global de errores
- SignalR básico
- Controllers de Auth, Ships, Products, BLs, Shifts, Receptions, Dispatches y Users
- Health endpoint
- Seeder seguro opcional de Admin

## Seguridad
- NO hay contraseña vacía.
- Jwt:Key queda vacío en appsettings.json.
- En desarrollo debes configurarlo con User Secrets:
  dotnet user-secrets set "Jwt:Key" "UNA-CLAVE-LARGA-Y-SEGURA-DE-AL-MENOS-32-CARACTERES"

## Admin inicial
Opcionalmente:
  dotnet user-secrets set "SeedAdmin:UID" "ADMIN"
  dotnet user-secrets set "SeedAdmin:FirstName" "ADMIN"
  dotnet user-secrets set "SeedAdmin:LastName" "IQBF"
  dotnet user-secrets set "SeedAdmin:Password" "UNA-CONTRASENA-INICIAL-SEGURA"

El seeder NO crea un Admin si UID o Password están vacíos.

## Importante
Aún NO ejecutar migraciones hasta compilar toda la solución y corregir cualquier referencia residual.
Después de compilar correctamente:
1. revisar proyectos/solution
2. crear migración inicial
3. crear IQBFControlDB_DEV
4. probar Swagger
5. probar Login + Start Shift + Reception + Dispatch
6. conectar frontend React
