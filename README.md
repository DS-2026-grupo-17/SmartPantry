# SmartPantry

Repositorio del Trabajo Práctico Integrador de Desarrollo de Software 2026.

## Integrantes
- Cipriano Lacava (ciprianolacava74-ux)
- Franco Ramirez (francoramirez7125-star)

## Requisitos

Antes de ejecutar el proyecto, cada integrante debe contar con:

- Visual Studio 2022 o Visual Studio 2026, con la carga de trabajo **Desarrollo de ASP.NET y web**.
- Node.js 24 LTS (versión 24.15.0 o superior). Angular 22 no es compatible con Node.js 22.11.0; verificar con `node --version`.
- Yarn 1.22.x disponible en la consola (usado por ABP para descargar las librerías del frontend). Verificar con `yarn --version`.
- SQL Server Developer o SQL Server Express instalado en forma local.
- SQL Server Management Studio (SSMS), para verificar las tablas creadas por ABP.
- Git y ABP Studio Desktop. Verificar Git con `git --version`.

## Configuración local

La solución usa Entity Framework Core con SQL Server. La cadena de conexión se configura directamente en dos archivos:

- `src/SmartPantry.DbMigrator/appsettings.json`
- `src/SmartPantry.HttpApi.Host/appsettings.json`

En ambos, dentro de `ConnectionStrings`, el valor de `Default` debe apuntar a una instancia local, por ejemplo:

```json
{
  "ConnectionStrings": {
    "Default": "Server=(localdb)\\MSSQLLocalDB;Database=SmartPantry;Trusted_Connection=True"
  }
}
```

Esta cadena usa autenticación integrada de Windows (sin contraseña), por lo que puede permanecer versionada como configuración local del proyecto.

> Si en el futuro se usa un servidor remoto, un usuario SQL o una contraseña, **no versionar** esa cadena: usar User Secrets o la variable de entorno `ConnectionStrings__Default`.

## Puesta en marcha

1. Restaurar dependencias del backend y frontend:
   ```bash
   abp install-libs
   dotnet restore .\SmartPantry.slnx
   ```
2. Compilar la solución (o usar Build Solution en Visual Studio, Ctrl+Shift+B):
   ```bash
   dotnet build .\SmartPantry.slnx --configuration Debug --no-restore
   ```
3. Ejecutar el migrador para crear la base de datos local (tablas base de ABP: usuarios, roles, permisos, auditoría):
   ```bash
   dotnet run --project .\src\SmartPantry.DbMigrator
   ```
4. Levantar el backend (HttpApi.Host):
   ```bash
   dotnet run --project .\src\SmartPantry.HttpApi.Host
   ```
   URL local: `https://localhost:44354/`

5. Levantar el frontend Angular:
   ```bash
   cd angular
   yarn start
   ```
   URL local: `http://localhost:4200/`

Para detener cada proceso, `Ctrl+C` en la terminal correspondiente (o detener la depuración en Visual Studio para el backend).

## Estructura de la solución

Aplicación monolítica en capas (ABP Application Layered), compuesta por:

- `angular/`: aplicación Angular (frontend).
- `src/SmartPantry.DbMigrator`: aplicación de consola que aplica las migraciones y datos iniciales.
- `src/SmartPantry.HttpApi.Host`: API ASP.NET Core que expone los endpoints consumidos por Angular.
- `src/SmartPantry.Domain`, `Application`, `EntityFrameworkCore`: capas de dominio, aplicación y persistencia.
- `test/`: proyectos de test (`Domain.Tests`, `Application.Tests`, `EntityFrameworkCore.Tests`).

La interfaz Angular consume el backend vía HTTP; no accede a la base de datos directamente.

## Verificación

Comandos ejecutados por el grupo para validar build y tests:

**.NET**
```bash
dotnet build .\SmartPantry.slnx --configuration Release --no-restore
dotnet test .\SmartPantry.slnx --configuration Release --no-build
```

**Angular**
```bash
cd angular
yarn build
yarn test --watch=false --browsers=ChromeHeadless
```

Persistencia verificada manualmente conectando SSMS a la instancia local y comprobando la existencia de las tablas base de ABP (`AbpUsers`, `AbpRoles`, `AbpPermissions`, `AbpAuditLogs`, entre otras) tras ejecutar el `DbMigrator`.
