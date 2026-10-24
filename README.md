# Nakama

Plataforma de onboarding interno para empresas: chatbots entrenados con la documentación de cada equipo/departamento y formaciones (currículos) guiadas por módulos, con seguimiento de progreso y analítica de uso.

## 📖 Descripción general

Nakama nace de un problema muy concreto que se repite con cada incorporación nueva: el tiempo que un desarrollador senior tiene que dedicarle no es solo el del primer día, es un goteo constante durante semanas — las mismas preguntas, los mismos errores de principiante, el mismo contexto explicado una y otra vez por cada junior que entra al equipo. Ese tiempo no escala, y es tiempo que el senior deja de dedicar a su propio trabajo.

Con el tiempo, gestionando varias incorporaciones, apareció un segundo problema más silencioso: los juniors recurren cada vez más a la IA generativa para resolver dudas de implementación, y muchas veces le hacen caso a ciegas — aceptan la respuesta sin entender el porqué, sin contrastarla con la documentación real del equipo ni con un criterio propio. Eso no es aprender, es delegar el pensamiento, y a la larga genera más deuda (de conocimiento, no solo de código) de la que resuelve.

Nakama ataca los dos frentes con una sola herramienta:

- **Chat con chatbots especializados**: cada equipo o departamento tiene su propio chatbot, entrenado solo con su documentación (sin mezclar contenido entre chatbots), con modo socrático opcional y respuestas con cita de la fuente — para que el junior encuentre la respuesta correcta sin interrumpir al senior, pero sin perder de vista de dónde sale esa respuesta.
- **Formaciones por módulos**: currículos compuestos por vídeos, documentos, ejercicios y exámenes, con bloqueo secuencial opcional (gating) entre módulos y seguimiento de progreso por persona — pensadas para guiar hacia la formación real antes que hacia la implementación rápida asistida por IA.

La idea no es prohibir el uso de IA, sino encauzarlo: que el primer reflejo de un junior sea consultar la documentación y la formación del equipo, no copiar lo primero que le devuelve un modelo.

Hay dos roles: **administrador** (crea y gestiona chatbots/formaciones, sube documentos, asigna equipos, ve analítica) y **persona del equipo** (usa el chat y cursa las formaciones que se le asignaron). La gestión de un recurso y su uso ("Gestionar" vs. "Cursar") son vistas separadas dentro de la misma app.

## 🛠️ Stack tecnológico

**Backend**
- [.NET 10](https://dotnet.microsoft.com/) / ASP.NET Core — Minimal API (patrón `IEndpoint` + [Scrutor](https://github.com/khellang/Scrutor) para registro automático)
- Arquitectura hexagonal (dominio/aplicación/infraestructura) + CQRS con [MediatR](https://github.com/jbogard/MediatR)
- [NHibernate](https://nhibernate.info/) como ORM, [FluentMigrator](https://fluentmigrator.github.io/) para migraciones de esquema
- [PostgreSQL 18](https://www.postgresql.org/) ([Npgsql](https://www.npgsql.org/) como driver)
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) (OpenAPI/Swagger) + [NSwag](https://github.com/RicoSuter/NSwag) para generar el cliente TypeScript del frontend a partir del contrato
- [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) para versionado de API

**Frontend**
- [React 19](https://react.dev/) + [TypeScript](https://www.typescriptlang.org/) (modo `strict`)
- [Vite](https://vitejs.dev/) como bundler/dev server
- [React Router](https://reactrouter.com/) para navegación
- Arquitectura por features (`domain` / `infrastructure` / `views`) con inyección de dependencias propia (sin librería externa)
- Context API para estado global (sin Redux/Zustand); CSS Modules con Sass (`.module.scss`) para estilos, sin UI kit externo
- [ESLint](https://eslint.org/) + [Prettier](https://prettier.io/) + [Stylelint](https://stylelint.io/)

**Infraestructura local**
- [Docker Compose](https://docs.docker.com/compose/) para levantar PostgreSQL

> Nota: el documento [`docs/architecture/stack-tecnologico.md`](docs/architecture/stack-tecnologico.md) recoge además decisiones de diseño **planificadas** (capa LLM vía Semantic Kernel, pgvector, multi-tenancy database-per-tenant, SSO con Jaltest Auth) que todavía no están implementadas en el código actual — este README describe lo que existe hoy.

## 🚀 Instalación y ejecución

### Pre-requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (la versión exacta está fijada en [`global.json`](global.json))
- [Node.js](https://nodejs.org/) 20+ y npm
- [Docker](https://www.docker.com/) (para PostgreSQL) — o una instancia de PostgreSQL 18 propia
- [dotnet-ef / herramientas locales](.config/dotnet-tools.json) del repo (`dotnet tool restore`) si vas a regenerar el cliente TypeScript con NSwag

### 1. Base de datos

```bash
docker compose up -d
```

Levanta PostgreSQL en `localhost:5432` (usuario/clave/BD: `nakama`/`nakama`/`nakama`, ver [`docker-compose.yaml`](docker-compose.yaml)).

### 2. Backend

Antes del primer arranque hay que definir la clave con la que se firman los JWT (mínimo 32 bytes). No va en `appsettings.json` ni en el repositorio: en desarrollo se guarda en los user-secrets de tu máquina, y en producción en la variable de entorno `Jwt__SigningKey`. Sin ella la aplicación **no arranca**.

```bash
cd apps/backend/api
dotnet user-secrets set "Jwt:SigningKey" "<cadena-aleatoria-de-al-menos-32-caracteres>"
dotnet run
```

Por defecto escucha en `https://localhost:7080` (ver [`Properties/launchSettings.json`](apps/backend/api/Properties/launchSettings.json)). Las migraciones de FluentMigrator (incluida la carga de datos de prueba) se aplican automáticamente al arrancar en entorno `Development`.

### 3. Frontend

```bash
cd apps/frontend/nakama.web
npm install
npm run dev
```

Levanta Vite en `http://localhost:3000`, con proxy de `/api` hacia el backend (ver [`vite.config.ts`](apps/frontend/nakama.web/vite.config.ts)).

### Comandos útiles

| Comando | Dónde | Qué hace |
|---|---|---|
| `dotnet build` | `apps/backend/api` o raíz | Compila el backend |
| `npm run build` | `apps/frontend/nakama.web` | `tsc -b` + build de producción con Vite |
| `npm run lint` / `lint-fix` | `apps/frontend/nakama.web` | Lint de TS/React |
| `npm run lint-css` | `apps/frontend/nakama.web` | Lint de estilos (Stylelint) |

> **Tests automatizados**: `apps/tests/Test` contiene los tests unitarios del backend (NUnit, Moq, AwesomeAssertions y Bogus) para los handlers de usuarios, autenticación y conversaciones, además del generador de tokens JWT. Se ejecutan con `dotnet test apps/tests/Test`; la convención a seguir está en `apps/tests/Test/agents.md`. Los tests de integración de la API y los del frontend están pendientes, y la verificación de esas partes sigue siendo manual (build limpio + comprobación en navegador).

## 📂 Estructura del proyecto

```
proyecto_master/
├── apps/
│   ├── backend/
│   │   ├── api/                  # Capa HTTP: endpoints Minimal API (IEndpoint), Program.cs
│   │   └── src/Nakama/           # Dominio/aplicación — un bounded context por carpeta:
│   │       ├── Chatbots/         #   agregado Chatbot + Conversation (escritura)
│   │       ├── ChatbotDetail/    #   proyección de lectura del detalle de un chatbot
│   │       ├── ChatbotSummaries/ #   proyección de lectura del listado de chatbots
│   │       ├── ConversationHistory/ # historial de conversaciones por usuario
│   │       ├── ConversationAudit/   # log de auditoría inmutable de conversaciones
│   │       ├── Formations/       #   agregado Formation (currículo y módulos)
│   │       ├── FormationEnrollment/ # agregado FormationEnrollment (asignación + progreso por persona)
│   │       ├── FormationDetail/  #   proyección de lectura del detalle de una formación
│   │       ├── FormationsList/   #   proyección de lectura del listado de formaciones (escopeado por usuario)
│   │       ├── Users/            #   usuarios y autenticación
│   │       ├── UserActivity/     #   actividad de usuario (logins y renovaciones) para analítica
│   │       ├── QuestionActivity/ #   preguntas a los chatbots (una fila por pregunta) para analítica
│   │       ├── AnswerFeedback/   #   valoración de cada respuesta (útil o no) para analítica
│   │       ├── Analytics/        #   agregación de métricas de uso
│   │       └── Shared/           #   infraestructura común (persistencia NHibernate, eventos, migraciones)
│   ├── frontend/nakama.web/
│   │   └── src/
│   │       ├── modules/<feature>/     # domain + infrastructure (contratos y repositorios HTTP)
│   │       └── views/
│   │           ├── pages/<feature>/   # páginas contenedor + hooks de caso de uso
│   │           ├── components/        # UI reutilizable
│   │           ├── shared/            # contextos globales, hooks y CSS transversal
│   │           └── routes/            # definición de rutas (React Router)
│   └── tests/                    # convención para tests .NET (NUnit + FluentAssertions) — aún sin implementar
├── docs/                         # ADRs, requisitos y documentación de arquitectura
├── infra/                        # Terraform del entorno de Azure (nakama-dev)
└── docker-compose.yaml           # PostgreSQL local
```

Cada módulo relevante tiene su propio `agents.md` con las convenciones de implementación de esa capa — ver [`CLAUDE.md`](CLAUDE.md) para la tabla de routing completa.

## ✨ Funcionalidades principales

- **Chat con chatbots por equipo**: conversaciones con modo socrático y explicación ampliada opcionales (el modelo cambia de estilo según el chatbot, y bajo la última respuesta aparecen los botones «Ver respuesta directa» y «Explícame más», que envían ese seguimiento a la misma conversación); historial de conversaciones por persona, retomable y borrable. Cada persona solo ve, consulta y pregunta a los chatbots de cuyo equipo forma parte; el administrador los ve todos.
- **Gestión de chatbots** (admin): alta y baja (en pantalla «Eliminar»; en realidad se archiva y un administrador puede reactivarlo desde el filtro «Eliminados», igual que las formaciones; un chatbot o formación dado de baja no admite preguntas ni cambios), estado (borrador/activo/inactivo), icono, ingesta de documentos, equipo con roles (propietario/editor/lector), y, en la pestaña de analítica, las preguntas de ese chatbot cuya respuesta se valoró como no útil esta semana.
- **Formaciones por módulos**: tipos vídeo, documento, ejercicio y examen (con editor de preguntas/opciones/respuesta correcta y documentos propios de la formación (PDF, Markdown o texto) y un borrador de hasta 4 preguntas generado con IA a partir de los que marques, con el proveedor y modelo que elijas; el módulo queda marcado como «Borrador IA» hasta que lo editas y lo guardas); bloqueo secuencial configurable entre módulos. Un examen necesita al menos una pregunta y una nota mínima entre 1 y 100 (el servidor lo exige y el formulario no propone ningún valor: la nota la escribe el administrador). Al crear una formación, la audiencia y la descripción son obligatorias. Los exámenes los corrige el servidor: quien cursa la formación entrega sus respuestas y no recibe nunca la respuesta correcta (solo las ven los administradores); cada intento queda registrado en su progreso, aprobado o no, y se puede reintentar hasta aprobar (un examen aprobado no se vuelve a entregar). Cada persona ve el historial de intentos de sus exámenes (fecha, nota y resultado) y los administradores lo ven de todo el equipo desde Permisos → Progreso del equipo → Ver exámenes (`GET /api/formations/{id}/enrollments`, solo administradores); no se guarda qué respondió cada persona a cada pregunta. Una formación dada de baja no admite completar módulos ni entregar exámenes.
- **Asignación y progreso por persona**: solo quien está asignado a una formación puede cursarla; el progreso (módulos completados, notas de examen) es individual por persona, no compartido entre quienes cursan la misma formación. El listado de `/formacion` se carga en función del usuario: una persona no-admin solo ve las formaciones a las que fue asignada; un admin ve todas, pero "Cursar" solo aparece en las que él mismo cursa.
- **Inicio (solo personas sin rol de administrador)**: página de aterrizaje `/inicio` con su resumen propio: bloque "Continúa" con la formación activa más avanzada y su siguiente módulo, "Tu última conversación" para retomarla en el chat, lista de pendientes (exámenes intentados y sin aprobar, formaciones sin empezar o a medias) y tres cifras (preguntas de la semana, días activo en los últimos 30 y formaciones completadas). Se alimenta de `GET /api/home`, que reutiliza la actividad de preguntas y de uso filtrada por persona y sus matrículas; las valoraciones de respuestas no se guardan por persona, así que no se muestra un % de respuestas útiles. Los administradores siguen aterrizando en Chat.
- **Estado del curso por persona**: quien no es administrador solo ve las formaciones **activas** que tiene asignadas (borrador e inactiva se le ocultan y el servidor rechaza completar módulos o entregar exámenes en ellas, `nakama.formations.not_active`) y las filtra por el estado de *su* curso: **En curso** (por defecto), **Completadas** o **Todas** (`progress=in_progress|completed` en `GET /api/formations/paged`). "Completada" se calcula, no se guarda: tiene módulos y los ha completado todos; si un admin añade un módulo vuelve a "En curso". El filtro de estado de la formación y el punto de estado son solo del administrador.
- **Vistas separadas Cursar / Gestionar**: la persona que cursa una formación usa `/formacion/:id` (progreso, módulos bloqueados, exámenes respondibles); un admin la configura en `/formacion/:id/gestionar` (alta/edición inline de módulos sin diálogo, equipo asignado) — son páginas y hooks independientes, no una sola vista condicionada por rol.
- **Listados paginados**: `/chatbots` y `/formacion` paginan, buscan y filtran por estado en el servidor (`GET /api/chatbots/paged` y `/api/formations/paged`); la página, el tamaño, la búsqueda y el estado viven en la URL (`?page=&size=&q=&status=`). El tamaño por defecto y el máximo se configuran en `Pagination` de `appsettings.json`; el frontend ofrece sus tamaños con `VITE_PAGINATION_PAGE_SIZE_OPTIONS` y `VITE_PAGINATION_DEFAULT_PAGE_SIZE` (ninguno puede superar el máximo del backend). Los endpoints sin paginar siguen existiendo para el resto de pantallas (ADR-26).
- **Analítica global**: personas activas, preguntas respondidas esta semana, preguntas por semana (gráfico de las últimas 4, 8 o 12 semanas, con `VITE_QUESTION_ACTIVITY_WEEK_OPTIONS` y `VITE_QUESTION_ACTIVITY_DEFAULT_WEEKS` como espejo de `Reporting:QuestionActivity:MaxWeeks` y `DefaultWeeks`, ADR-29), uso por chatbot del mes en curso (solo los que tuvieron preguntas), tasa de respuestas útiles, preguntas sin buena respuesta de la semana (las valoradas como no útiles) y actividad de usuarios (personas activas hoy, en 7 y en 30 días, y un gráfico diario de personas activas e inicios de sesión a 7, 30 o 90 días). Se alimenta de `UserActivity`, que registra cada login y cada renovación de sesión (como máximo una por persona y hora). Los días se cuentan en la zona horaria `Reporting:TimeZone`, única para toda la analítica; el frontend ofrece los rangos con `VITE_USER_ACTIVITY_DAY_OPTIONS` y `VITE_USER_ACTIVITY_DEFAULT_DAYS` (ninguno puede superar `Reporting:UserActivity:MaxDays`) (ADR-28). Cada respuesta del chat se puede valorar con 👍 o 👎, y la valoración se guarda en el intercambio, sobrevive a recargar el chat y se puede cambiar (ADR-30).
- **Autenticación propia**: login por email/contraseña contra la base de usuarios propia. Devuelve un JWT de acceso de 15 minutos, que se exige en todos los endpoints salvo login, refresh y logout; los de administración requieren además el rol de administrador. La sesión sobrevive a las recargas con un refresh token de 1 día en una cookie `HttpOnly` que se rota en cada uso (reutilizar uno ya rotado cierra todas las sesiones del usuario). Sin SSO todavía — ver nota de stack planificado más arriba.

## 🔑 Datos y cuentas de prueba

Las migraciones solo crean el esquema: una base nueva está **vacía y no hay ninguna cuenta con la que entrar** (la aplicación no tiene alta de usuarios). Para tener datos con los que explorarla, ejecuta a mano el script [`docs/db/demo-data.sql`](docs/db/demo-data.sql) una vez que la API haya aplicado las migraciones:

```bash
docker exec -i nakama-postgres psql -U nakama -d nakama < docs/db/demo-data.sql
```

El script se puede lanzar más de una vez (omite las filas que ya existen). Es un conjunto mínimo a propósito, para que la analítica parta de cero y refleje lo que hagas tú: dos cuentas de acceso y 3 personas más, 2 chatbots (uno activo y otro en borrador), 2 formaciones (una activa con un cuestionario y otra en borrador) y el proveedor Anthropic con el catálogo de modelos de Claude, desactivado y sin API key (pégala en «Proveedores de IA»). No trae conversaciones, valoraciones ni actividad de usuarios:

| Rol | Email | Contraseña |
|---|---|---|
| Administrador | `admin@nakama.com` | `Admin123.` |
| Persona de equipo | `user@nakama.com` | `Admin123.` |

Las otras 3 personas son `@seed.nakama.test` con la contraseña `Seed1234.`.

## 📦 Despliegue

La infraestructura de Azure (App Service Windows + PostgreSQL flexible server) se define con Terraform en [`infra/`](infra/README.md), junto con los pasos para publicar y configurar el entorno. Todavía no hay pipeline de CI/CD: el despliegue es manual.

## 🤝 Contribuyendo

Proyecto interno — antes de tocar un módulo, leé el `agents.md` de esa carpeta (ver tabla de routing en [`CLAUDE.md`](CLAUDE.md)) para seguir sus convenciones de nombres y capas.

## 📌 Versionado

Sin tags de versión todavía; se sigue el historial de commits en `main`.

## 📄 Licencia

Proyecto interno, sin licencia pública definida.
