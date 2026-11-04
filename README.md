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

## TP10 - Pantalla "Mi despensa" (RF-11)

Ruta Angular `/pantry` (componente `angular/src/app/pantry/PantryComponent`), protegida con `authGuard`
y registrada en el menú como `::Menu:MyPantry`. Consume el proxy generado `PantryItemService` (`@proxy/pantries`).

| RF | Pantalla / proxy | AppService.método | Endpoint |
|---|---|---|---|
| RF-11 Listar | `PantryItemService.getList` | `PantryItemAppService.GetListAsync` | `GET /api/app/pantry-item` |
| RF-11 Editar | `PantryItemService.update` | `PantryItemAppService.UpdateAsync` | `PUT /api/app/pantry-item/{id}` |
| RF-11 Consumir | `PantryItemService.markAsConsumed` | `PantryItemAppService.MarkAsConsumedAsync` | `POST /api/app/pantry-item/{id}/mark-as-consumed` |
| RF-11 Eliminar | `PantryItemService.delete` | `PantryItemAppService.DeleteAsync` | `DELETE /api/app/pantry-item/{id}` |
| RF-10 Agregar (sólo API) | `PantryItemService.create` | `PantryItemAppService.CreateAsync` | `POST /api/app/pantry-item` |

### Decisiones

- **Propiedad y acceso.** `PantryItemAppService` tiene `[Authorize]` y obtiene el propietario de `ICurrentUser`.
  `Pantry.OwnerId` es el Id del usuario de ABP Identity. Un ítem de otro usuario responde **404** en todas las
  operaciones (no se revela si existe). Ocultar botones en la UI no reemplaza esta comprobación.
- **Ítems consumidos.** Según la regla de negocio 6 se conservan como **historial**: aparecen con la etiqueta
  "Consumido" y atenuados, y el interruptor "Mostrar consumidos" permite ocultarlos (`includeConsumed`).
  Sólo se eliminan con la acción explícita "Eliminar", que pide confirmación.
- **ListService.** Se usa porque `GetListAsync` es paginado (`skipCount`/`maxResultCount`); la paginación se
  muestra con `ngb-pagination`, 10 ítems por página.
- **Criterio de fechas.** La fecha de vencimiento es una fecha de calendario (`DateOnly`, `yyyy-MM-dd`), sin hora
  ni zona horaria. La UI la muestra tal cual, con el formato regional del idioma activo (no la convierte desde UTC).
  El worker del TP08 compara esa misma fecha contra el **día UTC** actual; entre las 21:00 y las 24:00 de Argentina
  (UTC-3) el worker ya considera el día siguiente.
- **Manejo de errores.** El listado y el guardado usan `skipHandleError: true` porque el componente los muestra en
  la propia pantalla: estado de error con botón "Reintentar" y mensaje dentro del formulario sin perder lo
  ingresado. Marcar como consumido y eliminar usan el manejo predeterminado de ABP (mensaje modal).
- **Validación.** El servidor valida cantidad no negativa, unidad de hasta 20 caracteres y fecha entre los años 2000
  y 2100; la UI repite esas reglas sólo por usabilidad.
- **Datos faltantes (RF-09).** Si falta fecha, cantidad o unidad se muestra un texto localizado
  ("Sin fecha de vencimiento", "No disponible"), sin inventar valores.

### Ejecución local

1. `src/AbpSolution1.DbMigrator`: `dotnet run` (migraciones, cliente OpenIddict `AbpSolution1_App` y datos de ejemplo
   de la despensa del usuario `admin`).
2. Certificado HTTPS de desarrollo de confianza: `dotnet dev-certs https --trust`.
3. `src/AbpSolution1.HttpApi.Host`: `dotnet run`.
4. `angular`: `yarn install` y `yarn start`; abrir `http://localhost:4200`.
5. Pruebas: `dotnet test` (BackEnd) y `yarn test --watch=false` en `angular` (componente con mocks, sin red).
