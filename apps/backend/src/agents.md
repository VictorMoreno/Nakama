# Domain/Core Agent Guide

Guía exportable para mantener el núcleo de negocio con DDD + arquitectura hexagonal.

## 1) Arquitectura por bounded context

Cada módulo (Quotes, Sessions, Selection, etc.) se organiza así:

```text
<Module>/
  Application/
    <UseCase>/
  Domain/
    Errors/
    Events/
    Services/
  Infrastructure/
Shared/
  Domain/
  Infrastructure/
```

Reglas de dependencia:

- `Application` **no** depende de `Infrastructure`.
- `Infrastructure` **no** depende de `Application`.
- `Domain` **no** depende de `Application` ni `Infrastructure`.
- Un bounded context no debe depender de otro bounded context.

## 2) Patrón de casos de uso

- Usar clases estáticas por caso de uso (`FindCategory`, `SearchQuotes`, etc.).
- Estructura interna recomendada:
  - `Query`/`Command`
  - `Handler`
  - `Finder`/`Searcher`/`Creator`/`Updater`...
- Las clases de servicio de aplicación:
  - implementan `IService`
  - deben ser `sealed`
  - solo inyectan interfaces por constructor.

## 3) Dominio

- Entidades de dominio implementan `IYqEntity`.
- Propiedades de dominio con `private set`.
- Usar `partial` para separar:
  - archivo auto-generado (`Entity.cs`)
  - comportamiento/constructores (`Entity.domain.cs`)
- Errores de negocio en `Domain/Errors`, heredando de `DomainError`, con código estable (`yq.spare_parts...`).
- Eventos de dominio en `Domain/Events`.

## 4) Repositorios

- Interfaces en `Domain` con prefijo `I` y sufijo `Repository`.
- Implementaciones con nombre `{Prefijo}{NombreInterfaceSinI}` (ej: `NhSessionRepository`, `YqSpareRepository`).
- Firmas de interfaces de repositorio: devolver tipos del mismo bounded context (o `Task/Task<T>/List<T>` coherentes).
- En infraestructura, mapear DTOs externos a entidades/value objects de dominio.

## 5) Shared module

- `Shared.Domain`: contratos transversales (`ISystemClock`, `IUuidGenerator`, `ILanguageProvider`, etc.).
- `Shared.Infrastructure`: implementaciones técnicas (NH, clientes externos, notificaciones, language providers, migraciones).
- Reusar `NhRepositoryBase<TEntity>` para repositorios NH.

## 6) Persistencia y migraciones

- Migraciones en `Shared/Infrastructure/Persistence/Migrations` con timestamp en nombre.
- Mapping NH en `Persistence/Nh/Mappings`.
- Tipos complejos JSON en `Persistence/Nh/UserTypes`.

## 7) Convenciones de naming

- Interfaces: `I...`
- Repositorios: `...Repository`
- Casos de uso: verbo + contexto (`FindVehicleInfo`, `CreateSession`)
- Responses de aplicación: `...Response`
- Errores: `...Error` o `...Errors` según agregación.

## 8) Checklist para nueva funcionalidad de dominio

1. Crear/ajustar entidad(es) y value objects en `Domain`.
2. Añadir reglas y errores de dominio (`Domain/Errors`).
3. Definir contrato de repositorio en `Domain`.
4. Implementar caso de uso en `Application` con `Query/Command + Handler + Service`.
5. Implementar repositorio en `Infrastructure`.
6. Añadir mapping NH/migración si cambia persistencia.
7. Añadir/actualizar tests unitarios + arquitectura.

## 9) Comandos base

- Build: `dotnet build`
- Test unit/integration backend: `dotnet test .\test\Yq.SpareParts.Test\`
- Test arquitectura: `dotnet test .\test\Yq.SpareParts.Test.Architecture\`

