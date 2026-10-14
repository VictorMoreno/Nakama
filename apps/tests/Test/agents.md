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
        Infrastructure/                  <Adapter>Should (solo si hay lógica que probar; repositorios contra PostgreSQL real con TestContainers)
    Api/                                 integración de endpoints (pendiente)
  Test.Architecture/                     reglas de arquitectura (NetArchTest + reflexión)
```

- `Test`: pruebas unitarias de módulos e integración API. Referencia solo el proyecto del núcleo; la estructura de `Modules/` refleja la de `src/`.
- `Test.Architecture`: reglas de arquitectura y convenciones (NetArchTest + reflexión). Referencia `Nakama` y `Api`; no usa Mothers ni `UnitTestCase`.
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

### Dobles exactos, no permisivos

- Los helpers `ShouldFindX(entidad)` configuran el mock solo para la clave exacta de esa entidad (`FindById(entidad.Id)`, `FindByFormationAndUser(e.FormationId, e.UserId)`). Para el caso de no encontrado hay otro helper, `ShouldNotFindX()`, que responde `null` a cualquier clave. Nunca un único `ShouldFindX(entidad?)` con `It.IsAny`: dejaría pasar un handler que buscara por el id equivocado.
- Lo mismo para las queries del bus (`It.Is<Query>(q => q.Id == ...)`) y los colaboradores con argumentos de negocio (`Generate(user.Id, user.Email, ...)`, `Rotate(current, ..., now)`): el test construye el escenario con datos coherentes (el comando lleva el `Id` de la entidad simulada).
- `It.IsAny` queda para los `Verify` con `Times.Never` (ninguna llamada, sea cual sea el argumento), el `CancellationToken` y valores incidentales que genera el propio código (un `Guid` nuevo, un `Stream`).
- Los valores aleatorios que se comparan entre sí no pueden colisionar: dos `WordMother.Random()` coinciden de vez en cuando y vuelven el test intermitente. Si el escenario exige que sean distintos, derivar uno del otro (`$"{x}-other"`) o usar prefijos fijos.

### Infraestructura con base de datos real (TestContainers)

Los repositorios con SQL propio o consultas no triviales se prueban contra un PostgreSQL real (`postgres:18-alpine`, la misma imagen que `docker-compose.yaml`). Un mock no puede verificar un `update ... where revoked_at is null`.

- **Dónde:** `Modules/{Modulo}/Infrastructure/{Repositorio}Should.cs`, en el mismo proyecto `Test`. La clase hereda de `InfrastructureTestCase`, que ya trae la categoría `Infrastructure`.
- **Contenedor:** uno por ejecución, arrancado la primera vez que un test de infraestructura lo pide (los unitarios no tocan Docker). Aplica `MigrationRunner` en una base plantilla; las migraciones solo crean el esquema, así que la plantilla y cada base clonada nacen vacías. Ryuk, el contenedor auxiliar de Testcontainers, lo elimina si el proceso muere.
- **Una base por test:** en `[SetUp]` cada test clona la plantilla (`CREATE DATABASE ... TEMPLATE`, casi instantáneo) y en `[TearDown]` la borra. Ningún test ve lo que escribió otro, ni siquiera sus repeticiones, así que no hace falta limpiar tablas ni usar ids únicos. Las conexiones van sin `pool` para que el borrado no espere conexiones abiertas.
- **Fábrica de NHibernate por test:** cada test construye la suya (`NhWriteSessionFactory` con la cadena de su base) y la libera en `[TearDown]`; por eso la fábrica es `IDisposable`. `WriteSessions` ya la entrega y el repositorio se crea en el `[SetUp]` de la clase de test. Cuesta unos 0,4 s por test.
- **Qué probar:** el SQL y las condiciones que un mock no ve (atomicidad, concurrencia, filtros por usuario, hashes, mapeos). No repetir lo que ya cubren los tests de aplicación.
- **Fechas:** PostgreSQL guarda microsegundos y .NET 100 ns; comparar con `BeCloseTo(valor, TimeSpan.FromMilliseconds(1))`, nunca con `Be`.
- **Comprobarlo:** romper a propósito la línea SQL (quitar una condición del `where`) y ver que falla algún test, como con el resto.
- **Ejecutar:** necesita Docker en marcha. Solo unitarios: `dotnet test --filter "TestCategory!=Infrastructure"`. Solo infraestructura: `--filter "TestCategory=Infrastructure"`.

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

Están automatizadas en `apps/tests/Test.Architecture` (una clase `{Tema}Should` por tema: `LayeringShould`, `NamingShould`, `DomainShould`, `ApiShould`). Las reglas salen del código real, no de una plantilla: el proyecto usa Minimal API (`IEndpoint`) y MediatR, no controladores ni un bus propio.

**Capas** (`LayeringShould`, una prueba por módulo):

- `Domain` no depende de `Application`, de ninguna `Infrastructure` ni de frameworks (NHibernate, MediatR, ASP.NET Core, Microsoft.Extensions, FluentMigrator, Npgsql, Anthropic).
- `Domain` no depende del resto de módulos. Excepción documentada: `Chatbots.Domain` usa el enum `LlmProvider` de `LlmProviders` (`NakamaCode.AllowedDomainDependencies`).
- `Application` no depende de la `Infrastructure` de ningún módulo, ni de NHibernate, MediatR, ASP.NET Core, FluentMigrator, Npgsql ni Anthropic. Sí puede usar `Microsoft.Extensions.Options` y `DependencyInjection`.
- `Shared.Domain` y `Shared.Application` no conocen los módulos ni la infraestructura.
- Los módulos se hablan por eventos de dominio (`Domain.Events`) o por consultas del bus (`Application`), nunca por `Infrastructure`.

**Nombres y forma** (`NamingShould`):

- Interfaces empiezan por `I`; las implementaciones de repositorio son `Nh{NombreSinI}` y viven en `Infrastructure`.
- Eventos de dominio: `record` sellado en `.Domain.Events`. Errores de dominio: sellados y terminados en `Error`.
- Manejadores de eventos (`DomainEventHandler<>`): sellados, en `.Application.Handlers` y con `On` en el nombre.
- Manejadores de comandos y consultas: clase `Handler` sellada y anidada. Comandos y consultas sellados.
- Servicios (`IService`): sellados e inyectando solo interfaces o abstracciones genéricas (`IOptions<>`).

**Dominio** (`DomainShould`): las entidades no tienen setters públicos, viven en `.Domain`, tienen constructor sin parámetros `protected` (NHibernate) y un constructor público con todo su estado (los Mothers lo usan). Las clases `*Row` de los read models no son entidades y quedan fuera.

**Api** (`ApiShould`): cada `IEndpoint` es una clase sellada `*Endpoints` en `Api.Endpoints`; los endpoints no dependen de la `Infrastructure` de los módulos, de la persistencia ni de ningún `Domain`. Un endpoint solo lee y escribe datos HTTP (cookies, cuerpo) y delega las decisiones en el caso de uso; no lanza errores de dominio.

Al escribir una regla de dependencia con NetArchTest:

- Usar el namespace exacto de cada capa (`Nakama.{Modulo}.Domain`), nunca con punto final ni solo el prefijo del módulo: con punto final no encuentra nada y la regla pasa en vacío, y sin capa `Nakama.Formations` también casa con `Nakama.FormationsList`.
- Comprobar la regla rompiéndola a propósito (una referencia `typeof(...)` en un tipo de la capa) y viendo que falla. Es la única forma de detectar una regla que pasa en vacío: una regla sobre un conjunto vacío, o con una lista de prohibidos que no casa con nada, es verde siempre.
- Una excepción nueva se añade con nombre y motivo en `NakamaCode` o en la propia regla, nunca quitando la regla.

## 7) Checklist para nueva prueba

1. Elegir tipo de prueba (unitaria, integración módulo, API, arquitectura).
2. Reusar la base `<Module>UnitTestCase` existente y añadirle los helpers `ShouldX` que falten.
3. Crear los datos con `Mother`, nunca con helpers privados dentro del test.
4. Nombrar la clase `...Should` y el método en snake_case.
5. Configurar los dobles con la clave exacta (`ShouldFindX(entidad)`), no con `It.IsAny`.
6. Añadir casos positivos y negativos (incluyendo auth y errores cuando aplique), y en los negativos comprobar que no hubo efectos secundarios.
7. Comprobar que el test detecta el fallo: romper a propósito la línea de producción que cubre y ver que falla antes de restaurarla.
8. Ejecutar el proyecto de tests correspondiente.

## 8) Comandos base

- Ejecutar tests backend: `dotnet test .\apps\tests\Test\`
- Ejecutar tests arquitectura: `dotnet test .\apps\tests\Test.Architecture\`
- Ejecutar todo: `dotnet test` (ambos proyectos; los tests de infraestructura necesitan Docker)
- Sin Docker: `dotnet test .\apps\tests\Test\ --filter "TestCategory!=Infrastructure"`
- Con el IDE abierto, que bloquea `Nakama.dll`, compilar a un directorio temporal: `dotnet test .\apps\tests\Test\ -p:OutDir=<ruta-temporal>/` (usar una ruta distinta para cada proyecto de tests).
