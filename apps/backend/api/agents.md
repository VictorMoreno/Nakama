# API Agent Guide

Guía exportable para implementar y mantener APIs.

## Deriva conocida en `apps/backend/api`

Esta guía viene de otro proyecto ("Yq.SpareParts") y describe controladores MVC +
`ICommandBus`/`IQueryBus` custom (§3/§8). Lo que hay hoy en `apps/backend/api` es distinto y
deliberado: **Minimal API vía el patrón `IEndpoint` + Scrutor** (`Configuration/IEndpoint.cs`,
`Configuration/EndpointExtensions.cs`) para el ruteo HTTP, y **MediatR (`ISender`/`IPublisher`)**
en vez del bus custom — los endpoints inyectan `ISender` y llaman `sender.Send(new
<CasoDeUso>.Command(...))`/`sender.Send(new <CasoDeUso>.Query(...))` directo. No se migra a
controladores MVC salvo pedido explícito — el patrón `IEndpoint` ya está en marcha y activo.
El resto de esta guía (seguridad, pipeline HTTP, DI por extension methods, `ErrorMapping()`
para `DomainError -> HttpStatusCode`) sigue aplicando igual, solo cambia la superficie de
ruteo/dispatch. Ver `apps/backend/src/Nakama/agents.md` para las convenciones de dominio que
sí se siguen al pie de la letra.

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
  - `CwsControllerBase` (CWS)
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
- Run API: `dotnet run --project `
- Test backend: `dotnet test `

