# API Agent Guide

Convenciones reales de `apps/backend/api` — Minimal API. Ver `apps/backend/src/Nakama/agents.md`
para las convenciones de dominio/aplicación (se siguen al pie de la letra) y `ADR.md` en esa
misma carpeta para el porqué de las decisiones arquitectónicas referenciadas abajo.

## 1) Stack y arranque

- ASP.NET Core `net10.0`, Minimal API.
- `Program.cs` es deliberadamente fino: `AddInfrastructure()` + `AddApplication()` (DI), luego
  `app.Configure()` (pipeline HTTP) + `UseHttpsRedirection()` + `Run()`. Composición real vive en
  `Configuration/*Extensions.cs`, no en `Program.cs`.
- `AddInfrastructure()`: Swagger/versionado, registro de `IEndpoint` (Scrutor), health checks,
  `DomainErrorExceptionHandler` + `ProblemDetails`.
- `AddApplication()`: MediatR (solo para `IEventBus`/eventos de dominio — ver ADR-7), el scan de
  Scrutor que registra `ICommandHandler<>`/`IQueryHandler<,>`, `ICommandBus`/`IQueryBus`,
  NHibernate session factories, repositorios.

## 2) Ruteo — patrón `IEndpoint`, agrupado por bounded context

- Cada bounded context tiene **una** clase en `Endpoints/<BoundedContext>Endpoints.cs`
  implementando `Configuration/IEndpoint.cs` (`void MapEndpoints(IEndpointRouteBuilder app)`) —
  no una clase por módulo de `Application`. Los read-models que proyectan el mismo agregado
  (ej. `ChatbotSummaries`/`ChatbotDetail`/`ChatPage` para el bounded context Chatbots, ver ADR-9/ADR-10)
  agrupan en el mismo `IEndpoint` que los commands de su propio agregado.
- Se auto-registra vía el scan de Scrutor ya existente en `Configuration/EndpointExtensions.cs` —
  no hace falta tocar DI al agregar una clase nueva.
- `MapEndpoints` recibe el grupo `/api` y anida su propio subgrupo:
  `app.MapGroup("chatbots").WithTags("Chatbots")`. Las rutas dentro del método son relativas a
  ese subgrupo (`"/{id:guid}"`, nunca `"/api/chatbots/{id:guid}"`).

## 3) Inyección — solo `ICommandBus`/`IQueryBus`

- Los handlers de endpoint inyectan **únicamente** `ICommandBus`/`IQueryBus`
  (`Nakama.Shared.Domain`), nunca `ISender`/`IPublisher` de MediatR directo. MediatR queda oculto
  por diseño detrás de esos dos puertos (ver ADR-12b) — un endpoint no necesita saber que MediatR
  existe.
- Llamada: `await commandBus.Send(new <CasoDeUso>.Command(...), ct)` (siempre `Task`, un command
  nunca devuelve datos) / `await queryBus.Ask(new <CasoDeUso>.Query(...), ct)` (devuelve el
  `Response` tal cual).

## 4) Reglas de verbo HTTP / status code

Los ids de recursos nuevos los genera el caller (GUID v4), no el servidor (ver ADR-13b) — por eso
la creación es `PUT`, no `POST`: el id ya se conoce antes de la llamada.

| Caso | Verbo | Status éxito |
|---|---|---|
| Crear con id provisto por el cliente | `PUT /{recurso}/{id:guid}` | 201 + `Location` a la query de detalle |
| Reemplazar un campo/subrecurso completo (ej. Status, Settings) | `PATCH /{recurso}/{id:guid}/{campo}` | 204 |
| Acción de dominio sin campo directo (ej. Archive, Complete, ToggleGating) | `POST /{recurso}/{id:guid}/{acción}` | 204 |
| Agregar a una colección (ej. Documents, Team, Exchanges) | `POST /{recurso}/{id:guid}/{colección}` | 204 |
| Agregar a una colección con id propio provisto por el cliente (ej. Modules) | `PUT /{recurso}/{id:guid}/{colección}/{subId:guid}` | 201 |
| Quitar por índice de lista (sin id estable, solo posición) | `DELETE /{recurso}/{id:guid}/{colección}/{index:int}` | 204 |
| Listado | `GET /{recurso}` | 200 |
| Detalle por id (o una vista distinta del mismo agregado) | `GET /{recurso}/{id:guid}[/{vista}]` | 200 |

## 5) Request/Response DTOs

- Un `record` chico por command-con-body (`private sealed record <Verbo><Entidad>Request(...)`),
  anidado en el mismo archivo del endpoint — solo los campos que no vienen de la ruta. Si el
  record de dominio ya calza 1:1 con lo que necesita el body (ej. `ChatbotSettings`), se usa
  directo, sin duplicar un wrapper.
- Las `Response` de las queries se devuelven tal cual con `Results.Ok(response)` — ya son DTOs
  planos pensados para JSON, sin capa de mapeo adicional.

## 6) Documentación — `.WithSummary()`/`.WithDescription()`, no XML `///`

Un delegate de Minimal API no puede llevar comentarios XML de documentación, y el proyecto no
tiene wiring de XML-doc-a-OpenAPI (`Api.csproj` no declara `GenerateDocumentationFile`). La forma
idiomática de documentar acá es `.WithSummary("...")` (siempre) / `.WithDescription("...")` (solo
cuando el comportamiento no es obvio, ej. semántica de upsert) / `.Produces<T>()` /
`.ProducesProblem(...)`. Esto es intencional: en el resto del backend (`Domain`/`Application`/
`Infrastructure`) no hay comentarios `///` — viven en `ADR.md` cuando explican un "por qué" no
obvio — y `apps/backend/api` es la única capa donde sí se documenta en el código, porque acá
alimenta Swagger.

## 7) Errores de dominio — mapeo global, nunca try/catch por endpoint

`Configuration/DomainErrorExceptionHandler.cs` mapea cualquier `DomainError` lanzado por un
Handler a `ProblemDetails` con el status code correcto, por el sufijo de `DomainError.Code`
(`nakama.<módulo>.<razón>`): `.not_found` → 404, `.already_exists` → 409, cualquier otro → 422.
Un endpoint nunca necesita capturar `DomainError` por su cuenta. Excepciones que no son
`DomainError` (ej. `ArgumentOutOfRangeException` por un índice de lista inválido) caen al 500
default de ASP.NET Core — no están mapeadas a propósito.

## 8) Checklist para endpoints nuevos

1. Ubicar el caso de uso en su `IEndpoint` de bounded context (o crear uno nuevo si es un bounded
   context distinto a los existentes).
2. Elegir verbo/ruta según la tabla de §4.
3. Handler estático: parámetros de ruta + `Request` (si hay body) + `ICommandBus`/`IQueryBus` + `ct`.
4. `.WithSummary()`/`.Produces<T>()`/`.ProducesProblem(...)` — sin XML `///`.
5. Confirmar que el `DomainError` que puede lanzar el caso de uso mapea al status code esperado
   (§7) — si es un código nuevo, no hace falta tocar el exception handler, mapea solo por sufijo.
6. **Autorización: pendiente, explícitamente fuera de alcance por ahora.** No agregar
   `[Authorize]`/middleware de auth hasta que se pida — cuando se implemente, este es el último
   punto del checklist, no el primero.

## 9) Comandos base

- Build: `dotnet build Nakama.slnx` (desde la raíz del repo)
- Run API: `dotnet run --project apps/backend/api`
