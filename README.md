# Nakama

Plataforma de onboarding interno para empresas: chatbots entrenados con la documentación de cada equipo/departamento y formaciones (currículos) guiadas por módulos, con seguimiento de progreso y analítica de uso.

## 📖 Descripción general

Nakama nace de un problema muy concreto que se repite con cada incorporación nueva: el tiempo que un desarrollador senior tiene que dedicarle no es solo el del primer día, es un goteo constante durante semanas — las mismas preguntas, los mismos errores de principiante, el mismo contexto explicado una y otra vez por cada junior que entra al equipo. Ese tiempo no escala, y es tiempo que el senior deja de dedicar a su propio trabajo.

Con el tiempo, gestionando varias incorporaciones, apareció un segundo problema más silencioso: los juniors recurren cada vez más a la IA generativa para resolver dudas de implementación, y muchas veces le hacen caso a ciegas — aceptan la respuesta sin entender el porqué, sin contrastarla con la documentación real del equipo ni con un criterio propio. Eso no es aprender, es delegar el pensamiento, y a la larga genera más deuda (de conocimiento, no solo de código) de la que resuelve.

Nakama ataca los dos frentes con una sola herramienta:

- **Chat con chatbots especializados**: cada equipo o departamento tiene su propio chatbot, entrenado solo con su documentación (sin mezclar contenido entre chatbots), con modo socrático opcional y respuestas con cita de la fuente — para que el junior encuentre la respuesta correcta sin interrumpir al senior, pero sin perder de vista de dónde sale esa respuesta.
- **Formaciones por módulos**: currículos compuestos por vídeos, documentos, ejercicios y exámenes, con bloqueo secuencial opcional (gating) entre módulos y seguimiento de progreso por persona — pensadas para guiar hacia la formación real antes que hacia la implementación rápida asistida por IA.

La idea no es prohibir el uso de IA, sino encauzarlo: que el primer reflejo de un junior sea consultar la documentación y la formación del equipo, no copiar lo primero que le devuelve un modelo.

Hay dos roles: **administrador** (crea y gestiona chatbots/formaciones, sube documentos, asigna equipos, ve analítica) y **persona del equipo** (usa el chat y cursa las formaciones que se le asignaron). La gestión de un recurso y su uso ("Gestionar" vs. "Abrir") son vistas separadas dentro de la misma app.

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
│   │       ├── ChatPage/         #   guion de ejemplo (opening/chips) del chat
│   │       ├── ConversationHistory/ # historial de conversaciones por usuario
│   │       ├── ConversationAudit/   # log de auditoría inmutable de conversaciones
│   │       ├── Formations/       #   agregado Formation (currículo y módulos)
│   │       ├── FormationEnrollment/ # agregado FormationEnrollment (asignación + progreso por persona)
│   │       ├── FormationDetail/  #   proyección de lectura del detalle de una formación
│   │       ├── FormationsList/   #   proyección de lectura del listado de formaciones (escopeado por usuario)
│   │       ├── Users/            #   usuarios y autenticación
│   │       ├── UserActivity/     #   actividad de usuario para analítica
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
└── docker-compose.yaml           # PostgreSQL local
```

Cada módulo relevante tiene su propio `agents.md` con las convenciones de implementación de esa capa — ver [`CLAUDE.md`](CLAUDE.md) para la tabla de routing completa.

## ✨ Funcionalidades principales

- **Chat con chatbots por equipo**: conversaciones con citación de fuente, modo socrático y explicación ampliada opcionales; historial de conversaciones por persona, retomable y borrable.
- **Gestión de chatbots** (admin): alta/baja, estado (borrador/activo/inactivo), icono, ingesta de documentos, equipo con roles (propietario/editor/lector), analítica de preguntas sin respuesta (*gaps*) y preguntas más frecuentes.
- **Formaciones por módulos**: tipos vídeo, documento, ejercicio y examen (con editor de preguntas/opciones/respuesta correcta); bloqueo secuencial configurable entre módulos.
- **Asignación y progreso por persona**: solo quien está asignado a una formación puede cursarla; el progreso (módulos completados, notas de examen) es individual por persona, no compartido entre quienes cursan la misma formación. El listado de `/formacion` se carga en función del usuario: una persona no-admin solo ve las formaciones a las que fue asignada; un admin ve todas, pero "Abrir" solo aparece en las que él mismo cursa.
- **Vistas separadas Abrir / Gestionar**: la persona que cursa una formación usa `/formacion/:id` (progreso, módulos bloqueados, exámenes respondibles); un admin la configura en `/formacion/:id/gestionar` (alta/edición inline de módulos sin diálogo, equipo asignado) — son páginas y hooks independientes, no una sola vista condicionada por rol.
- **Analítica global**: chatbots activos, preguntas respondidas, uso por bot, tasa de respuestas útiles.
- **Autenticación propia**: login por email/contraseña contra la base de usuarios propia. Devuelve un JWT de acceso de 15 minutos, que se exige en todos los endpoints salvo login, refresh y logout; los de administración requieren además el rol de administrador. La sesión sobrevive a las recargas con un refresh token de 7 días en una cookie `HttpOnly` que se rota en cada uso (reutilizar uno ya rotado cierra todas las sesiones del usuario). Sin SSO todavía — ver nota de stack planificado más arriba.

## 🔑 Usuario y contraseña de prueba

El arranque en entorno `Development` siembra automáticamente dos cuentas (ver [`SeedUsersMigration.cs`](apps/backend/src/Nakama/Shared/Infrastructure/Persistence/Migrations/SeedUsersMigration.cs)):

| Rol | Email | Contraseña |
|---|---|---|
| Administrador | `admin@nakama.com` | `Admin123.` |
| Persona de equipo | `user@nakama.com` | `Admin123.` |

Además se siembran chatbots y formaciones de ejemplo con datos realistas (ver [`SeedDemoTestDataMigration.cs`](apps/backend/src/Nakama/Shared/Infrastructure/Persistence/Migrations/SeedDemoTestDataMigration.cs)).

## 📦 Despliegue

Todavía no hay pipeline de CI/CD ni configuración de despliegue en el repositorio. El flujo actual es ejecución local (backend + frontend + PostgreSQL vía Docker Compose, como se describe arriba).

## 🤝 Contribuyendo

Proyecto interno — antes de tocar un módulo, leé el `agents.md` de esa carpeta (ver tabla de routing en [`CLAUDE.md`](CLAUDE.md)) para seguir sus convenciones de nombres y capas.

## 📌 Versionado

Sin tags de versión todavía; se sigue el historial de commits en `main`.

## 📄 Licencia

Proyecto interno, sin licencia pública definida.
