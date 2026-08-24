# API Agent Guide

Guía exportable para implementar y mantener APIs con el estilo de `Yq.SpareParts.Api`.

## 1) Stack y arranque

- ASP.NET Core `net9.0`.
- Composición en `Startup`:
  - `AddApi(configuration)`
  - `AddApplication()`
  - `AddInfrastructure(configuration)`
- `Program.cs` centraliza carga de configuración, `UpdateDatabase()` y `RunAsync()`.

## 2) Estructura recomendada

```text
<ApiProject>/
  Config/
    Auth/
    Extensions/
    Swagger/
  Controllers/
    <Feature>/
    Shared/
  Filters/
  Health/
  Extensions/
  Resources/
```

## 3) Reglas de controladores (obligatorias)

- Heredar de `ApiControllerBase` y usar bases de auth:
  - `YqControllerBase` (YQ JWT)
  - `GijControllerBase` (GIJ JWT)
  - `CwsControllerBase` (CWS)
  - `YqGijControllerBase` (mixto)
- El nombre debe terminar en método HTTP + `Controller`:
  - `*GetController`, `*PostController`, `*PutController`, `*PatchController`, `*DeleteController`.
- Todos los controladores deben ser `sealed`.
- El constructor del controlador solo debe inyectar `ICommandBus` y/o `IQueryBus`.
- Declarar `[Tags("...")]`, `ProducesResponseType`, y atributos de ruta explícitos.
- Si hay errores de dominio, sobrescribir `ErrorMapping()` para mapear `DomainError -> HttpStatusCode`.

## 4) Seguridad y autorización

- Definir autenticación en `Config/Auth` y wiring en `AddJwtAuthorization` + `AddCws`.
- Controlar permisos por roles usando constantes en `AuthorizationRoles`.
- Evitar `AllowAnonymous` salvo excepciones explícitas (ejemplo típico: health checks).

## 5) Pipeline HTTP

- Registrar `ApiExceptionFilter` global para normalizar errores.
- Mantener `RouteTokenTransformerConvention` para rutas consistentes.
- Usar `BasicAuthorizationMiddleware` + `UseAuthentication()` + `UseAuthorization()`.
- Exponer health checks por endpoint dedicado (`/api/health`) con writer unificado.

## 6) DI y extensiones

- Usar extension methods para encapsular bootstrap:
  - `AddApi`, `AddApplication`, `AddInfrastructure`.
- Registrar handlers y services por escaneo de ensamblado (Scrutor + helper de assemblies).
- Evitar lógica de inicialización compleja en `Program.cs`; moverla a `Config/Extensions`.

## 7) Convenciones de naming

- Controllers: `<Feature><Action><HttpMethod>Controller`.
- Filtros/Middlewares: sufijo `Filter` / `Middleware`.
- Extensions: sufijo `Extensions`.
- Health checks: sufijo `HealthCheck`.
- Clases de configuración por integración: nombres explícitos (`AzureGraphConfig`, `CwsAuthorization`).

## 8) Integración con aplicación/dominio

- El controlador no contiene lógica de negocio: solo orquestación HTTP + bus.
- Entrada por query/route/body -> comando/query de aplicación.
- Salida siempre con DTO de capa Application.

## 9) Checklist para endpoints nuevos

1. Crear controlador `sealed` en `Controllers/<Feature>`.
2. Inyectar solo `ICommandBus/IQueryBus`.
3. Declarar atributos de auth, tags, route y responses.
4. Implementar mapping de errores en `ErrorMapping()` si aplica.
5. Registrar cualquier dependencia técnica nueva en `Config/Extensions`.
6. Añadir pruebas API e impacto en pruebas de arquitectura.

## 10) Comandos base

- Build: `dotnet build`
- Run API: `dotnet run --project .\apps\backend\Yq.SpareParts.Api\`
- Test backend: `dotnet test .\test\Yq.SpareParts.Test\`

