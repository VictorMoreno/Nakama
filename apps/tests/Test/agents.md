# Testing Agent Guide

Guía exportable para pruebas en proyectos .NET con NUnit + Moq + AwesomeAssertions + Bogus, incluyendo pruebas de arquitectura.

## 1) Tipos de pruebas y estructura

```text
apps/tests/
  Test/                                  (Test.csproj, namespace raíz Test)
    Modules/
      Shared/                            UnitTestCase, MotherCreator, WordMother...
      <Module>/
        <Module>UnitTestCase.cs
        Application/                     <UseCase>Should, <UseCase>CommandMother | <UseCase>QueryMother
        Domain/                          <Entity>Mother (sin pruebas directas de entidades)
        Infrastructure/                  <Adapter>Should (solo si hay lógica que probar)
    Api/                                 integración de endpoints (pendiente)
  Test.Architecture/                     reglas de arquitectura (pendiente)
```

- `Test`: pruebas unitarias de módulos e integración API. Referencia solo el proyecto del núcleo; la estructura de `Modules/` refleja la de `src/`.
- `Test.Architecture`: reglas de arquitectura y convenciones (NetArchTest + reglas custom).
- No se prueban entidades ni value objects de dominio de forma aislada: su comportamiento se valida a través de los casos de uso. La carpeta `Domain/` de los tests contiene los Mothers.
- Las pruebas de infraestructura solo se escriben si hay lógica significativa (por ejemplo, un generador de tokens o un repositorio con consultas complejas). Contra bases de datos reales usar TestContainers; contra servicios HTTP externos, MockingServer.

## 2) Convenciones de naming

- Clases de prueba: sufijo `Should`, `sealed` y `[TestFixture]` (`UserFinderShould`, `LogRefresherShould`).
- Métodos de prueba: snake_case descriptivo, legible como una frase (`fail_when_the_refresh_token_is_expired`).
- Builders/fakers de datos: patrón `Mother`:
  - `<Entity>Mother` en `Domain/` (`UserMother`, `RefreshTokenMother`).
  - `<UseCase>CommandMother` o `<UseCase>QueryMother` en `Application/` (`UserCredentialsVerifierQueryMother`).
  - Mothers de objetos de respuesta, junto al módulo que los define (`UserResponseMother`).
- Bases de prueba: `UnitTestCase` (compartida) y `<Module>UnitTestCase` por módulo (`UsersUnitTestCase`).
- Namespaces: `Test.Modules.<Module>.<Layer>`.
- El handler bajo prueba se guarda en un campo `_handler` creado en un `[SetUp]`.

## 3) Pautas para unit tests de Application

- Heredar de `<Module>UnitTestCase`, que hereda de `UnitTestCase`.
- `UnitTestCase` aporta solo los mocks compartidos (`ISystemClock`, `IEventBus`) y los reinicia en un `[SetUp]`. No fija una hora por defecto: el test que dependa de ella la declara con `ShouldBeNow(now)` dentro del propio test, nunca en el `[SetUp]` ni en un campo de la clase, para que quien lo lea vea el escenario completo.
- `<Module>UnitTestCase` aporta los mocks del módulo (repositorio, buses, generadores) y los helpers con nombre de intención: `ShouldFindUserByEmail`, `ShouldHaveCreated`, `ShouldNotHavePublishedAnything`. El test nunca llama a Moq directamente.
- Mockear dependencias con `Moq` (repositorios, buses, reloj, puertos). Lo que es lógica propia del módulo se ejecuta de verdad; solo se simulan los puertos.
- Construir el handler pasando al `Creator`/`Finder`/`Remover` anidado los mocks del test case.
- Estructura de cada test: preparar con helpers `ShouldX`, ejecutar el handler, verificar con helpers `ShouldHaveX`.
- Verificar los errores de dominio por tipo (`ThrowAsync<UserEmailAlreadyInUseError>()`), sin comprobar código ni mensaje. Comprobar el mensaje solo cuando lleva datos del fallo que el test debe demostrar que se propagan.
- En los casos de fallo, comprobar también que no hubo efectos secundarios: nada guardado, nada publicado, nada rotado.
- Usar `AwesomeAssertions` para equivalencia y excepciones.
- No extraer a métodos privados del test fragmentos de preparación ni de verificación (generar un token, construir parámetros de validación...): el escenario se lee completo dentro de cada test, aunque haya algo de repetición. Lo reutilizable va a Mothers y a los helpers `ShouldX` del test case.
- Sin `var`, con `this.` en miembros y sin comentarios.

```csharp
[Test]
public async Task fail_without_recording_activity_when_the_password_is_wrong()
{
    User user = UserMother.Random();
    this.ShouldBeNow(DateTime.UtcNow);
    this.ShouldFindUserByEmail(user);
    this.ShouldVerifyPassword(false);

    Func<Task> act = async () => await this._handler.Handle(
        UserCredentialsVerifierQueryMother.With(user.Email),
        CancellationToken.None);

    await act.Should().ThrowAsync<UserInvalidCredentialsError>();
    user.LastActiveAt.Should().BeNull();
    this.ShouldNotHaveSavedAnyUser();
    this.ShouldNotHavePublishedAnything();
}
```

## 4) Pautas para tests API

- Construir el host con `WebApplicationFactory<Program>` y sustituir servicios por dobles de prueba.
- Verificar autenticación y roles con casos explícitos (200, 401, 403) y, para recursos ajenos, 404.
- Comprobar los errores como `ProblemDetails` con su `type` (`nakama.<recurso>.<situacion>`).
- Usar las mismas opciones JSON que la API para una serialización consistente.
- En escenarios asíncronos o eventuales, reintentar la comprobación con un temporizador limitado en lugar de dormir.

## 5) Pautas para datos de prueba

- Centralizar la aleatoriedad en `MotherCreator` (`Bogus`) y construir sobre ella los Mothers de valores (`WordMother`).
- Un Mother expone `Random()` y `With(...)`, cuyos parámetros son opcionales y nulos: cada campo no indicado se rellena con un valor aleatorio. Así cada test especifica solo lo que le importa.
- Los datos que el test necesita comparar se obtienen del objeto creado por el Mother (`command.Id`, `command.InitialPassword`), no de literales repetidos.
- Preferir valores aleatorios a literales: demuestran que el dato viaja desde la entrada hasta la salida y no está fijado en el código. Si se usan en `WithMessage`, generarlos sin comodines (`*`, `?`).
- Para construir estados inválidos o intermedios (token caducado, revocado o ya rotado), los agregados exponen un constructor público con todos los parámetros y el Mother lo usa. El camino de producción sigue siendo el método de fábrica (`Create`, `Start`), que llama a ese constructor con los valores por defecto.
- Mantener builders deterministas cuando el caso requiere aserciones exactas.

## 6) Reglas de arquitectura (deben seguirse siempre)

Aún no están automatizadas: el proyecto `Test.Architecture` está pendiente.

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
2. Reusar la base `<Module>UnitTestCase` existente y añadirle los helpers `ShouldX` que falten.
3. Crear los datos con `Mother`, nunca con helpers privados dentro del test.
4. Nombrar la clase `...Should` y el método en snake_case.
5. Añadir casos positivos y negativos (incluyendo auth y errores cuando aplique), y en los negativos comprobar que no hubo efectos secundarios.
6. Comprobar que el test detecta el fallo: romper a propósito la línea de producción que cubre y ver que falla antes de restaurarla.
7. Ejecutar el proyecto de tests correspondiente.

## 8) Comandos base

- Ejecutar tests backend: `dotnet test .\apps\tests\Test\`
- Ejecutar tests arquitectura (cuando exista): `dotnet test .\apps\tests\Test.Architecture\`
- Ejecutar todo: `dotnet test`
- Con el IDE abierto, que bloquea `Nakama.dll`, compilar a un directorio temporal: `dotnet test .\apps\tests\Test\ -p:OutDir=<ruta-temporal>/`
