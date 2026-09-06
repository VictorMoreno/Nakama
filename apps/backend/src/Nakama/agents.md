# Domain/Core Agent Guide

Guía exportable para mantener el núcleo de negocio con DDD + arquitectura hexagonal.

## 1) Arquitectura por bounded context

Cada módulo (Quotes, Sessions, Selection, etc.) es una carpeta de primer nivel, hermana de
`Shared/` — **sin ningún nivel `Modules/` agrupador por encima**. Se organiza así:

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
- Un bounded context no debe depender de otro bounded context — con la única excepción de
  §7 (modelos de lectura), donde la `Application` de un módulo de lectura puede referenciar
  los `Domain/Events` (nunca las entidades/agregados) de un bounded context de escritura, para
  alimentar su propia proyección.

## 2) Patrón de casos de uso

- Usar clases estáticas por caso de uso, nombradas **contexto + rol** (`VehicleInfoFinder`,
  `SessionCreator`), no verbo + contexto — el rol es el sufijo agente de la operación:
  `Finder`/`Searcher`/`Creator`/`Updater`/`Archiver`/`Adder`/`Remover`/`Appender`/`Toggler`/
  `Completer`... (el que corresponda al verbo real de la operación).
- **`Finder` vs `Searcher` — no son intercambiables**:
  - `Finder`: busca **un** registro puntual por un criterio que debería identificarlo de forma
    única (key, singleton). Su respuesta **nunca es nullable** — si no lo encuentra, el
    `Handler` lanza un `DomainError` dedicado del propio bounded context (`?? throw
    <Contexto>Errors.NotFound(key)`), mismo patrón que ya usan los `Command` cuando no
    encuentran el agregado a mutar. No encontrar lo que se busca por su identificador es una
    situación excepcional, no un resultado válido.
  - `Searcher`: puede devolver **0, 1 o muchos** elementos (`IReadOnlyCollection<...>`) —
    nunca lanza excepción por "no encontrado", una colección vacía es un resultado válido y
    esperado. Úsalo para cualquier query que no busque por identificador único (listados,
    filtros, búsquedas).
- Estructura interna recomendada:
  - `Query`/`Command`
  - `Response` (solo para `Command` — ver más abajo el caso de `Query`)
  - `Handler`
- **Response de un `Command`**: nested dentro del propio caso de uso, en el mismo archivo —
  cada comando puede necesitar devolver algo distinto (p. ej. la key recién creada, o vacío).
- **Response de un `Query`**: NO es nested del caso de uso. Es un tipo compartido
  `<Agregado>Response`, en su **propio archivo** (`Application/<Agregado>Response.cs`, al
  nivel de `Application/`, no dentro de la carpeta de ningún caso de uso concreto) —
  reutilizado por **todos** los casos de uso `Query` de ese bounded context, porque todos
  devuelven la forma del mismo agregado. Lleva una conversión `explicit operator` desde la
  entidad de dominio/lectura hacia el response, para transformar de forma directa
  (`(ChatbotSummaryResponse)entity`). El `Query` del caso de uso apunta directo a ese tipo (o a
  una colección del mismo) como su `IRequest<T>` — sin volver a envolverlo en un `Response`
  nested propio. Ejemplo:
  ```csharp
  // ChatbotsList/Application/ChatbotSummaryResponse.cs
  namespace Nakama.ChatbotsList.Application;

  public sealed record ChatbotSummaryResponse(string Key, string Label, string Status, int DocsCount, int TeamCount)
  {
      public static explicit operator ChatbotSummaryResponse(ChatbotSummary entity) =>
          new(entity.Key, entity.Label, entity.Status, entity.DocsCount, entity.TeamCount);
  }

  // ChatbotsList/Application/Searcher/ChatbotSummarySearcher.cs — Searcher, no Finder: devuelve
  // 0..N elementos (un listado), no busca un único registro por identificador.
  public static class ChatbotSummarySearcher
  {
      public sealed record Query : IRequest<IReadOnlyCollection<ChatbotSummaryResponse>>;

      public sealed class Handler(IChatbotSummaryRepository chatbotSummaryRepository)
          : IRequestHandler<Query, IReadOnlyCollection<ChatbotSummaryResponse>>
      {
          public async Task<IReadOnlyCollection<ChatbotSummaryResponse>> Handle(Query query, CancellationToken ct)
          {
              var entries = await chatbotSummaryRepository.FindAll(ct);
              return entries.Select(e => (ChatbotSummaryResponse)e).ToList();
          }
      }
  }

  // ChatbotDetail/Application/Finder/ChatbotDetailFinder.cs — Finder: busca UN registro por
  // key, nunca nullable, lanza si no existe.
  public static class ChatbotDetailFinder
  {
      public sealed record Query(string Key) : IRequest<ChatbotDetailResponse>;

      public sealed class Handler(IChatbotDetailRepository chatbotDetailRepository) : IRequestHandler<Query, ChatbotDetailResponse>
      {
          public async Task<ChatbotDetailResponse> Handle(Query query, CancellationToken ct)
          {
              var chatbot = await chatbotDetailRepository.FindByKey(query.Key, ct)
                  ?? throw ChatbotDetailErrors.NotFound(query.Key);
              return (ChatbotDetailResponse)chatbot;
          }
      }
  }
  ```
- Carpeta del caso de uso: el nombre de la clase **sin el prefijo de contexto/agregado** —
  si la operación no tiene un calificador propio (p. ej. `AnalyticsFinder`, único Finder del
  módulo `Analytics`), la carpeta es solo el rol: `Finder/`. Si la operación sí tiene un
  calificador que forma parte de su propio nombre (p. ej. `ChatbotStatusUpdater`, porque
  "Status" no es el contexto sino qué se actualiza), ese calificador se mantiene en la
  carpeta: `StatusUpdater/`. Ejemplos completos:
  - `Analytics/Application/Finder/AnalyticsFinder.cs` (singleton — Finder)
  - `ChatbotsList/Application/Searcher/ChatbotSummarySearcher.cs` (listado — Searcher)
  - `Chatbots/Application/Creator/ChatbotCreator.cs`
  - `Chatbots/Application/StatusUpdater/ChatbotStatusUpdater.cs`
  - `Chatbots/Application/SettingsUpdater/ChatbotSettingsUpdater.cs`
  - `Chatbots/Application/TeamMemberAdder/ChatbotTeamMemberAdder.cs`
- Las clases de servicio de aplicación:
  - implementan `IService`
  - deben ser `sealed`
  - solo inyectan interfaces por constructor.

## 3) Dominio

- Entidades de dominio implementan `IEntity` (equivalente Nakama del `IYqEntity` de la guía
  original — no se reutiliza el nombre de otro proyecto). Los agregados de escritura además
  extienden `AggregateRoot<TId> : IEntity`; las entidades de lectura (§7) implementan `IEntity`
  directamente, nunca `AggregateRoot<TId>`.
- Propiedades de dominio con `private set` (getter `virtual` cuando la entidad se persiste con
  NHibernate y necesita soportar proxies de lazy-loading — `virtual` no obliga a relajar el
  `private set`, NHibernate accede al setter privado igual vía reflexión).
- **Usar `partial` para separar SIEMPRE en dos archivos, en TODA entidad de dominio — tanto
  agregados de escritura como entidades de lectura (§7), sin excepción**:
  - `<Entidad>.cs` — mitad "estructural": declaraciones de propiedades, constantes/config
    estática si las hay (p. ej. `Analytics.SingletonKey`), y el constructor protegido sin
    argumentos que pide NHibernate para proxies/hidratación — sin cuerpo, no es comportamiento
    de negocio, es plumbing estructural igual que las propiedades.
  - `<Entidad>.domain.cs` — comportamiento real: los factories (`Create`/`From`) y los métodos
    de negocio/mutación (`UpdateStatus`, `Apply`, etc.), nunca el constructor vacío. `.domain`
    es el sufijo común elegido para todas las entidades — no varía por módulo ni por tipo de
    entidad (agregado o ReadModel).
  Ejemplo: `Analytics/Domain/Analytics.cs` (propiedades + ctor NHibernate) +
  `Analytics/Domain/Analytics.domain.cs` (`From`/`Apply`); mismo split en
  `Chatbots/Domain/Chatbot.cs`/`.domain.cs`, `ChatbotsList/Domain/ChatbotSummary.cs`/`.domain.cs`,
  etc.
- **Toda entidad de dominio construye con un constructor `private` con todos los parámetros de
  negocio, nunca con `new() { Prop = valor, ... }` en el factory — agregados de escritura Y
  entidades de lectura (§7) por igual**: una entidad de lectura también necesita una creación
  real para poder persistirse, aunque no extienda `AggregateRoot<TId>`. El constructor vive en
  `.domain.cs` (es comportamiento, igual que `Create`/`From`), asigna cada propiedad en su
  cuerpo, y es `private` — el único punto que lo invoca es el propio `Create`/`From` de la
  entidad, manteniendo ese factory como la única puerta de entrada pública para construir la
  entidad con datos. Ejemplo en un agregado de escritura (`Chatbots/Domain/Chatbot.domain.cs`):
  ```csharp
  private Chatbot(ChatbotKey key, string label, string description, ChatbotStatus status,
      bool general, ChatbotSettings settings, DateTime lastUsed)
  {
      this.Id = key;
      this.Label = label;
      this.Description = description;
      this.Status = status;
      this.General = general;
      this.Settings = settings;
      this.LastUsed = lastUsed;
  }

  public static Chatbot Create(ChatbotKey key, string label, string description,
      ChatbotStatus status, bool general, ChatbotSettings settings, ISystemClock systemClock)
  {
      var chatbot = new Chatbot(key, label, description, status, general, settings, systemClock.UtcNow);
      chatbot.Raise(new ChatbotCreated(key, label, description, status, general));
      return chatbot;
  }
  ```
  `Id` (heredado de `AggregateRoot<TId>`) tiene `protected set`, así que el constructor —parte
  de la misma clase `partial`— lo asigna sin problema. Mismo criterio en una entidad de lectura
  (`ChatbotDetail/Domain/ChatbotDetail.domain.cs`), sin `AggregateRoot`/`Raise`, solo asignar y
  devolver:
  ```csharp
  private ChatbotDetail(string key, string label, string description, string status, bool general)
  {
      this.Key = key;
      this.Label = label;
      this.Description = description;
      this.Status = status;
      this.General = general;
  }

  public static ChatbotDetail Create(string key, string label, string description, string status, bool general) =>
      new(key, label, description, status, general);
  ```
- Errores de negocio en `Domain/Errors`, heredando de `DomainError`, con código estable
  (`nakama.<módulo>...` — adaptado del `yq.spare_parts...` de la guía original). `DomainError`
  hereda directamente de `Exception` (`Shared/Domain/DomainError.cs`) — no hay una
  `DomainException` intermedia que lo envuelva: el propio error concreto ES la excepción, y se
  lanza tal cual desde el método de dominio que detecta la violación (`throw
  ChatbotErrors.OwnerCannotBeRemoved;`, no `throw new DomainException(...)`). No es un
  `Result<T>`: es el mecanismo elegido para que la capa API (`ErrorMapping()`) tenga un tipo
  concreto (`DomainError`) que capturar y mapear a `HttpStatusCode`.
- Cada bounded context que hace una búsqueda "not found" (un `Finder`, o un `Command` que
  busca el agregado que va a mutar) declara su propio `<Contexto>NotFoundError : DomainError`
  más un método estático `<Contexto>Errors.NotFound(key)` (no una propiedad cacheada, porque
  el mensaje necesita la key) en su propio `Domain/Errors/<Contexto>Errors.cs`, y lanza eso en
  vez de un `InvalidOperationException` genérico. `InvalidOperationException` queda reservado
  para fallos que no son de dominio (p. ej. errores de infraestructura/configuración como
  `NhSessionFactoryBuilder`), nunca para "entidad no encontrada".
- Eventos de dominio en `Domain/Events`.

## 4) Repositorios

- Interfaces en `Domain` con prefijo `I` y sufijo `Repository`.
- Implementaciones con nombre `{Prefijo}{NombreInterfaceSinI}` (ej: `NhSessionRepository`, `YqSpareRepository`).
- Firmas de interfaces de repositorio: devolver tipos del mismo bounded context (o `Task/Task<T>/List<T>` coherentes).
- En infraestructura, mapear DTOs externos a entidades/value objects de dominio.
- **Toda implementación `Nh...Repository` extiende `NhRepositoryBase<TEntity, TId>`** (§5) —
  no repite `OpenSession()`/`BeginTransaction()`/`SaveOrUpdateAsync()` a mano. El método
  público de la interfaz (`FindByKey`, `FindByBotKey`, `FindSingleton`...) es una línea que
  delega en el `FindById` (o `FindAll`) `protected` del base — el nombre público sigue siendo
  el del dominio, no el genérico del base:
  ```csharp
  public sealed class NhChatbotSummaryRepository(ISessionFactory readSessions)
      : NhRepositoryBase<ChatbotSummary, string>(readSessions), IChatbotSummaryRepository
  {
      public Task<ChatbotSummary?> FindByKey(string key, CancellationToken ct) => this.FindById(key, ct);
      public Task<IReadOnlyCollection<ChatbotSummary>> FindAll(CancellationToken ct) => this.FindAllEntities(ct);
      // Save se hereda directo del base, sin redeclarar
  }
  ```

## 5) Shared module

- `Shared.Domain`: contratos transversales (`ISystemClock`, `IUuidGenerator`, `ILanguageProvider`, etc.).
- `Shared.Infrastructure`: implementaciones técnicas (NH, clientes externos, notificaciones, language providers, migraciones).
- **`NhRepositoryBase<TEntity, TId>`** (`Shared/Infrastructure/Persistence/Nh/NhRepositoryBase.cs`):
  base genérica que centraliza la mecánica NHibernate repetida en cada repositorio —
  `Save`/`Update` (upsert vía `SaveOrUpdateAsync`; `Update` es un alias semántico sobre el
  mismo mecanismo, NHibernate no distingue insert/update en `SaveOrUpdate`), `Remove`
  (`DeleteAsync`, disponible aunque hoy ningún repo borra físicamente — se archiva vía el
  propio agregado, no vía el repositorio), y un `FindById`/`FindAllEntities` `protected` (no
  `public`: cada repo expone su propio nombre de dominio, nunca el genérico, hacia su
  interfaz). Recibe un `ISessionFactory` común — `IWriteSessionFactory`/`IReadSessionFactory`
  extienden esa interfaz compartida (mismo `OpenSession(): ISession`, distinto token de DI),
  así el mismo base sirve para repos de escritura y de lectura sin duplicarlo.

## 6) Persistencia y migraciones

- Migraciones en `Shared/Infrastructure/Persistence/Migrations` con timestamp en nombre.
- Mapping NH en `Persistence/Nh/Mappings`.
- Tipos complejos JSON en `Persistence/Nh/UserTypes` — **decisión consciente de alcance, no
  definitiva**: colecciones anidadas (documentos, equipo, exchanges, módulos, quiz...) se
  mapean como columna JSON (`JsonUserType<T>`) en vez de mappings NH `bag`/componente
  normalizados. Es más simple hoy, pero renuncia a poder hacer query NH directa dentro de esas
  colecciones (p. ej. "todos los chatbots con un documento de tal título"). Si en algún
  momento hace falta consultar dentro de una de estas colecciones, ahí corresponde migrar esa
  colección concreta a un mapping `bag`/componente normal — no antes.
- Las proyecciones de ReadModels **no** viven aquí: cada una vive en la `Application` de su
  propio módulo de lectura, no centralizadas en `Persistence` — ver §7.
- `NhRepositoryBase<TEntity, TId>` centraliza la mecánica de sesión/transacción — ver §4/§5.
- **Pendiente, no resuelto todavía**: database-per-tenant (una base Postgres completa por
  departamento, ya decidido a nivel de producto) implica que `IWriteSessionFactory`/
  `IReadSessionFactory` no pueden ser una única instancia global con connection string fija —
  hace falta resolverlas por tenant. La wiring actual construye la `ISessionFactory` de forma
  lazy (primer `OpenSession()`, nunca al armar el contenedor DI) precisamente para no bloquear
  esa resolución más adelante, pero el mecanismo de resolución por tenant en sí **no está
  construido** — no asumir que ya existe.

## 7) Modelos de lectura (CQRS)

Un ReadModel **es un bounded context más**, con la misma forma que cualquier otro (§1): su
propia carpeta de primer nivel junto a `Shared/`, con `Domain`/`Application`/`Infrastructure`.
No es una subcarpeta dentro del módulo del agregado que lo alimenta.

- La entidad de lectura implementa `IEntity` (nunca `AggregateRoot<TId>`): no levanta eventos
  propios, es la consumidora final de eventos de otros agregados, no una productora.
- **Nombre de la entidad de lectura: siempre en singular** — una instancia representa un único
  registro proyectado, nunca la colección completa, y no lleva sufijo técnico que delate que
  "es una proyección" (`Entry`/`ReadModel`/`Row`): se nombra como un concepto de dominio, igual
  que `ChatbotDetail`/`ChatPage`/`FormationDetail`. Coincide con el nombre del bounded context
  solo cuando ese nombre ya es singular. Cuando el bounded context describe una vista de
  listado y por eso es plural (`ChatbotsList`, `FormationsList` — una por ruta de listado:
  `/chatbots`, `/formacion`), la entidad **no** hereda ese plural: se nombra
  `<Agregado>Summary` — `ChatbotSummary` en `ChatbotsList/Domain/ChatbotSummary.cs`,
  `FormationSummary` en `FormationsList/Domain/FormationSummary.cs` (una fila = el resumen de
  un agregado para esa lista). Mismo criterio que ya usa el lado de escritura, donde el módulo
  `Chatbots` (plural, área de dominio) contiene el agregado `Chatbot` (singular): el nombre del
  módulo describe el bounded context, el nombre de la entidad describe UN registro — nunca al
  revés.
- Se alimenta con un factory `Create(propiedades individuales)` (primera vez) y métodos
  `Apply(propiedades individuales)` (actualizaciones) — **nunca reciben el evento de dominio
  completo como parámetro**, solo los valores primitivos/value objects que necesitan. El
  `Domain` de un ReadModel no lleva ningún `using` hacia el `Domain.Events` de otro bounded
  context: esa referencia es exclusiva de la `Application` (el handler, ver más abajo), que sí
  conoce el evento, extrae sus campos y se los pasa a la entidad ya desagregados — el handler
  actúa como capa anticorrupción entre el evento ajeno y el Domain propio, evitando que un
  agregado quede acoplado a la forma interna del evento de otro. La lógica de "cómo afecta cada
  valor a esta proyección" sigue viviendo en la propia entidad, no en el handler — el handler
  solo traduce evento→valores, nunca decide lógica de negocio.
- **`Apply` nunca se usa pelado — siempre se nombra por la acción que hace** (`UpdateStatus`,
  `UpdateSettings`, `AddDocument`, `RemoveDocument`, `MarkArchived`, `IncrementDocsCount`...):
  al quitar el evento del parámetro, la mayoría de las mutaciones de una misma entidad terminan
  compartiendo tipo de parámetro entre sí (dos campos `string`, dos índices `int`...), así que
  el overload `Apply(mismoTipo)` colisiona en la práctica más seguido de lo que no — mantenerlo
  "genérico solo cuando no colisiona" produce una clase con algunos métodos `Apply` y otros con
  nombre propio sin ningún criterio visible para quien lo lee. Nombrar siempre por la acción es
  además más expresivo (`UpdateStatus(string status)` dice más que `Apply(string status)`) y es
  el criterio estándar de DDD para métodos de agregado — un ReadModel no es una excepción.
- **`Create` sí se mantiene pelado**, salvo que dos eventos distintos bootstrapeen la misma
  entidad con parámetros del mismo tipo — colisión real, no solo potencial (p. ej. `Analytics`,
  singleton sin evento de creación propio, se bootstrapea desde `ChatbotCreated` o desde
  `ConversationExchangeAppended`, ambos con un único `string botKey`): ahí sí se sufija con el
  evento de origen, `CreateFromChatbotCreated(string botKey)` /
  `CreateFromConversationExchangeAppended(string botKey)`. Fuera de ese caso raro, cada entidad
  tiene un único evento que la crea, así que `Create(...)` sin sufijo no colisiona con nada.
  Ninguna de estas dos reglas afecta el nombre de la **clase handler**
  (`<Entidad><Efecto>On<Evento>` sigue siendo `Created`/`Updated` según corresponda) — solo al
  método del `Domain` que esa clase invoca.
- Contrato de repositorio (`I...Repository`) en `Domain`, igual que cualquier agregado.
  Implementación (`Nh...Repository`) en `Infrastructure` del propio módulo de lectura.
- **Los handlers de evento (`INotificationHandler<DomainEventNotification<TEvent>>`) que
  alimentan estas entidades viven en la `Application` del propio módulo de lectura** — no son
  un detalle técnico de persistencia, son orquestación: reaccionan a un evento y deciden qué
  hacer, igual que el `Handler` de un `Command` (§2), solo que el disparador es un evento de
  dominio en vez de una petición del usuario. No se centralizan en `Shared` ni viven en
  `Infrastructure`: cada handler pertenece conceptualmente al módulo cuya entidad mantiene.
- **Una clase por handler, nunca una clase con muchas interfaces**: cada evento que afecta a
  una entidad de lectura tiene su propia clase, implementando un único
  `INotificationHandler<DomainEventNotification<TEvent>>`. Carpeta `Application/Handlers/`
  (agrupa todos los handlers de evento del módulo, no solo "proyecciones" de lectura — mismo
  criterio que `Application/Finder/` o `Application/Creator/`: la carpeta nombra el rol, no la
  técnica), nombre `<Entidad><Efecto>On<Evento>` — `Created` cuando ese evento es el que construye la
  entidad por primera vez (llama a `<Entidad>.Create(...)`, pasando los campos del evento uno
  a uno), `Updated` cuando solo la muta (`<Entidad>.Apply(...)`, mismo criterio). Ejemplo: el
  evento `ChatbotCreated` alimentando `Analytics` es
  la clase `AnalyticsCreatedOnChatbotCreated`; el evento `ChatbotStatusChanged` alimentando
  `ChatbotDetail` es `ChatbotDetailUpdatedOnChatbotStatusChanged`. Si un módulo de lectura
  mantiene más de una entidad interna (p. ej. `FormationDetail` también mantiene su propia
  `ChatbotLabelRef` de denormalización, ver más abajo), el nombre usa la entidad que ESE
  handler concreto muta, no el nombre del módulo — `ChatbotLabelRefCreatedOnChatbotCreated`,
  nunca `FormationDetailCreatedOnChatbotCreated`. Sin `Mutate` compartido entre handlers de
  eventos distintos: cada clase hace su propio `find → Apply/From → Save` inline (el `Handler`
  de un `Command` tampoco extrae ese tipo de helper compartido entre casos de uso, §2 — mismo
  criterio). MediatR descubre estas clases por ensamblado (`RegisterServicesFromAssembly`), no
  hace falta registrarlas a mano en DI por vivir fuera de `Infrastructure`. Cada handler
  accede al repositorio del módulo de lectura correspondiente (su propio `Domain`, inyectado)
  — nunca abre una `ISession` a pelo ni contiene reglas de negocio propias — y es el único
  punto que puede referenciar los `Domain/Events` (nunca las entidades/agregados) del bounded
  context de escritura que lo alimenta: es la única excepción a la regla de dependencia entre
  bounded contexts (§1), acotada a la `Application` — extrae del evento los valores que la
  entidad necesita y se los pasa individualmente a `Create`/`Apply`, nunca el evento en sí.
- Query de lectura en `Application` del propio módulo de lectura, mismo patrón contexto+rol +
  `Query`/`Handler` que cualquier caso de uso (§2) — `Finder` si busca un único registro
  (p. ej. `ChatbotDetailFinder` por key, `AnalyticsFinder` por ser singleton) o `Searcher` si
  devuelve un listado (p. ej. `ChatbotSummarySearcher`) — mismo criterio Finder-vs-Searcher de
  §2, un ReadModel no es una excepción. Todos comparten el mismo `<Agregado>Response` de §2
  (nunca un `Response` nested propio, porque un ReadModel también es un agregado a estos
  efectos).
- **Denormalización cross-módulo entre dos ReadModels**: si el ReadModel A necesita un dato
  que "pertenece" a otro ReadModel B (p. ej. `FormationDetail` necesita el label del chatbot
  vinculado, que en teoría "vive" en `ChatbotsList`), **no** lee el repositorio de B — eso
  violaría la regla de dependencia (§1: ningún bounded context referencia el `Domain` de otro,
  ReadModels incluidos). En vez de eso, A modela su propia entidad interna, pequeña, dentro de
  su propio `Domain` (p. ej. `FormationDetail/Domain/ChatbotLabelRef.cs`), alimentada
  escuchando directamente los eventos del bounded context de escritura de origen (`Chatbot`),
  igual que cualquier proyección — nunca leyendo el ReadModel ajeno.

## 8) Convenciones de naming

- Interfaces: `I...`
- Repositorios: `...Repository`
- Casos de uso: contexto + rol (`VehicleInfoFinder`, `SessionCreator`), no verbo + contexto —
  carpeta = nombre de clase sin el prefijo de contexto (§2). `Finder` = un único registro,
  nunca nullable, lanza si no existe; `Searcher` = 0..N elementos, nunca lanza (§2).
- Responses de `Command`: `...Response`, nested del propio caso de uso.
- Responses de `Query`: `<Agregado>Response`, un archivo propio en `Application/`, compartido
  por todos los Query del bounded context, con `explicit operator` desde la entidad (§2).
- Errores: `...Error` o `...Errors` según agregación.
- Entidades de lectura: siempre en singular — coincide con el bounded context solo cuando éste
  ya es singular, o `<Agregado>Summary` cuando el bounded context es un listado plural (§7).
- **Parámetros/campos inyectados por constructor: el nombre de la interfaz sin el prefijo `I`,
  en camelCase — nunca una abreviatura que no revele de qué instancia se trata.**
  `IChatbotDetailRepository` → `chatbotDetailRepository`, `IUuidGenerator` → `uuidGenerator`,
  `ISystemClock` → `systemClock`, `IEventBus` → `eventBus`. Nunca `repo`, `ids`, `clock` a
  secas: son abreviaturas que no dicen a qué repositorio/generador se refieren, y una clase con
  dos repositorios inyectados (p. ej. `FormationDetailCreatedOnFormationCreated`, que inyecta
  `IFormationDetailRepository` + `IChatbotLabelRefRepository`) directamente los necesita
  distinguibles — `formationDetailRepository`/`chatbotLabelRefRepository`, nunca dos parámetros
  llamados `repo`. Esta regla no es exclusiva de parámetros de constructor: aplica a cualquier
  variable tipada a una interfaz `I...`, incluyendo parámetros de factory methods de dominio
  (`Chatbot.Create(..., ISystemClock systemClock)`, no `clock`).

## 9) Estilo de código C#

- **Siempre usar `this.` al referenciar un miembro de instancia** (propiedad o campo
  declarado en la clase) desde dentro de un método de esa misma clase — `this.AnsweredThisWeek`,
  no `AnsweredThisWeek` a secas. Aplica a lecturas y escrituras por igual. No aplica a
  parámetros de constructor primario capturados como estado implícito (`Handler(IFooRepository
  repo)` → `repo` se usa tal cual, sin `this.`, porque no es una propiedad/campo declarado en
  el cuerpo de la clase) ni, por supuesto, a miembros `static`.
- **Todo bloque va entre llaves, siempre** — `if`/`else`/`for`/`foreach`/`while`/`using`, sin
  excepción, incluso si el cuerpo es una sola línea:
  ```csharp
  // Mal
  if (entry is null) return;

  // Bien
  if (entry is null)
  {
      return;
  }
  ```

## 10) Checklist para nueva funcionalidad de dominio

1. Crear/ajustar entidad(es) y value objects en `Domain`.
2. Añadir reglas y errores de dominio (`Domain/Errors`).
3. Definir contrato de repositorio en `Domain`.
4. Implementar caso de uso en `Application` con `Query/Command + Handler + Service`.
5. Implementar repositorio en `Infrastructure`.
6. Añadir mapping NH/migración si cambia persistencia.
7. Si el caso de uso afecta a un ReadModel existente (o crea uno nuevo): añadir/ajustar la
   entidad de lectura (§7) y su handler `<Entidad><Efecto>On<Evento>` en
   `<ReadModel>/Application/Handlers/`.
8. Añadir/actualizar tests unitarios + arquitectura.

## 11) Dependencias con licencia a vigilar

- **MediatR: fijado en la línea 13.x (Community Edition)**, no subir a mayor sin re-chequear
  licencia. MediatR pasó a licencia dual (RPL-1.5 + comercial) desde la v13 — la Community
  Edition es gratis mientras Nakama facture menos de $5M/año en el momento de la build; la
  v12.5.0 fue la última versión Apache totalmente libre pero ya no recibe actualizaciones. Se
  eligió 13.x conscientemente sobre 12.5.0 — no volver a esa versión sin que alguien lo pida
  explícitamente.

## 12) Comandos base

- Build: `dotnet build`
- Test unit/integration backend: `dotnet test .\test\Yq.SpareParts.Test\`
- Test arquitectura: `dotnet test .\test\Yq.SpareParts.Test.Architecture\`

