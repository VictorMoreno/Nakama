# Testing Agent Guide

Guía exportable para pruebas en proyectos .NET con NUnit + FluentAssertions, incluyendo pruebas de arquitectura.

## 1) Tipos de pruebas y estructura

```text
test/
  <Project>.Test/
    Api/
    Modules/
    Helpers/
    Resources/
  <Project>.Test.Architecture/
    Architecture/
      Rules/
```

- `Yq.SpareParts.Test`: pruebas unitarias, de integración de módulos e integración API.
- `Yq.SpareParts.Test.Architecture`: reglas de arquitectura y convenciones (NetArchTest + reglas custom).

## 2) Convenciones de naming

- Clases de prueba: sufijo `Should` (`FindCategoryShould`, `QuotesSearchGetControllerShould`).
- Métodos de prueba: snake_case descriptivo (`not_find_index_if_empty_or_null_token_filter`).
- Builders/fakers de datos: patrón `Mother` (`CategoryMother`, `UserMother`, `QuoteResponseMother`).
- Bases de prueba por contexto: `...TestCase` / `...ApiContext`.

## 3) Pautas para unit tests de Application/Domain

- Heredar de `UnitTestCase` o `<Feature>TestCase` cuando exista.
- Mockear dependencias con `Moq` (repositorios, buses, reloj, UUID, language provider).
- Preparar escenarios con helpers `ShouldX(...)` del test case.
- Verificar errores de dominio por tipo, mensaje y `ErrorCode`.
- Usar `FluentAssertions` para equivalencia y excepciones.

## 4) Pautas para tests API

- Heredar de contexto adecuado:
  - `YqApiContext` (auth YQ)
  - `GijApiContext` (auth GIJ)
  - `CwsApiContext` (auth CWS2)
- Construir host con `ApiBuilder.Build<Program>()` y override de servicios para test doubles.
- Verificar autenticación/roles con casos explícitos (200, 401, 403).
- Usar `JsonOptions` común para serialización consistente.
- En escenarios asíncronos/eventuales usar `Eventually(...)`.

## 5) Pautas para datos de prueba

- Centralizar randomización en `MotherCreator` (`Bogus`).
- Usar `ListMother`, `GuidMother`, `DateMother`, etc. para evitar literals repetidos.
- Mantener builders deterministas cuando el caso requiere aserciones exactas.

## 6) Reglas de arquitectura (deben seguirse siempre)

- Interfaces empiezan por `I`.
- Implementaciones de repositorios respetan `{Prefijo}{NombreSinI}`.
- Controladores terminan en `(Get|Post|Put|Patch|Delete)Controller`.
- Controladores autorizados (sin `AllowAnonymous`, salvo excepciones explícitas).
- Controladores solo inyectan `ICommandBus`/`IQueryBus`.
- Servicios (`IService`) sellados e inyectando solo interfaces.
- Entidades de dominio con setters no públicos.
- Capas sin dependencias prohibidas (hexagonal + DDD).

## 7) Checklist para nueva prueba

1. Elegir tipo de prueba (unitaria, integración módulo, API, arquitectura).
2. Reusar `TestCase/Context` existente de la feature.
3. Crear datos con `Mother`/helpers.
4. Nombrar clase `...Should` y método en snake_case.
5. Añadir casos positivos y negativos (incluyendo auth/errores cuando aplique).
6. Ejecutar proyecto de tests correspondiente.

## 8) Comandos base

- Ejecutar tests backend: `dotnet test .\test\Yq.SpareParts.Test\`
- Ejecutar tests arquitectura: `dotnet test .\test\Yq.SpareParts.Test.Architecture\`
- Ejecutar todo: `dotnet test`

