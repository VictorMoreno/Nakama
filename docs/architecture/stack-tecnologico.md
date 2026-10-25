# Stack Tecnológico

**Fecha:** 2026-08-15
**Estado:** Decidido (fase de diseño)
**Basado en:** `docs/requirements/analisis-requisitos-chatbot-onboarding.md`, `docs/research/guia-entrevistas-validacion.md`

---

## 1. Resumen de decisiones

| Capa | Elección | Motivo |
|------|----------|--------|
| Frontend | React + TypeScript | Elección directa del equipo |
| Backend | .NET 10 (ASP.NET Core Web API) | Elección directa del equipo |
| Arquitectura | Hexagonal (Ports & Adapters) + CQRS (MediatR) | Separa casos de uso (commands: ingesta/chat, queries: consulta) de infraestructura; permite intercambiar proveedores sin tocar el dominio |
| Puerto LLM | `ILlmChatProvider` / `IEmbeddingProvider` (interfaces de dominio) | El dominio no conoce el proveedor concreto; se configura por chatbot/departamento |
| Adaptador LLM | Semantic Kernel como implementación interna del puerto (conectores OpenAI/Anthropic/Azure OpenAI/modelo local) | Evita reinventar la integración con LLMs; cubre RAG, embeddings y function calling de forma nativa en .NET |
| Fallback | Microservicio Python solo si una funcionalidad no está disponible en .NET/Semantic Kernel | P. ej. OCR pesado o preprocesado NLP muy específico |
| Persistencia + Vector Store | PostgreSQL + pgvector, detrás de `IVectorStorePort` | Empieza simple (pgvector); si no rinde lo suficiente, se sustituye por Qdrant/Pinecone sin tocar el dominio |
| Ingesta de documentos | Parsers .NET (PdfPig para PDF, Markdig para Markdown) | Evita depender de Python salvo casos raros |
| Multi-tenancy | Database-per-tenant (una base de datos PostgreSQL por departamento) | Aislamiento fuerte a nivel físico (cumple RNF1: 0 cruces de información); el tenant se resuelve a nivel de conexión, no de filtro lógico |
| Autenticación | Servidor de identidad propio | Permite arrancar sin dependencias externas, con la autenticación detrás de un puerto/interfaz para no acoplar el dominio |

---

## 2. Multi-tenancy: database-per-tenant

- Cada departamento tiene su propia base de datos PostgreSQL (datos relacionales + pgvector para embeddings).
- La resolución de tenant (qué departamento/BD corresponde a la petición) ocurre en la capa de infraestructura, antes de llegar al dominio — el dominio y los casos de uso (CQRS) no conocen la conexión física.
- Ventaja: aislamiento físico total (RNF1), y evita fugas por error de filtrado lógico (a diferencia de un esquema con `tenant_id` compartido).
- Coste a vigilar: más bases de datos que gestionar/migrar a medida que crecen los departamentos (a tener en cuenta en el plan de migraciones y en el pipeline de despliegue).

## 3. Autenticación

- Servidor de identidad propio (a definir: Duende IdentityServer u OpenIddict sobre .NET 10) para no bloquear el desarrollo por una dependencia externa.
- El acceso a autenticación se modela como puerto (interfaz) desde el arranque, de forma que cambiar el proveedor sea un cambio de adaptador/configuración, no una reescritura.

## 4. Capa LLM configurable por bot

- `ILlmChatProvider` e `IEmbeddingProvider` son las interfaces de dominio que usan los casos de uso (chat, ingesta).
- Cada chatbot/departamento tiene configuración propia de qué proveedor/modelo consumir (RF1: gestión de chatbots por departamento).
- Semantic Kernel se usa dentro del adaptador de infraestructura como implementación de esos puertos, no como dependencia del dominio.
- Si alguna funcionalidad no está cubierta por Semantic Kernel, se evalúa consumir un microservicio en Python detrás del mismo puerto, sin que el dominio se entere del cambio.

---

## 5. Decisiones pendientes

- [ ] Elegir implementación concreta del servidor de identidad propio (Duende IdentityServer vs OpenIddict)
- [ ] Estrategia de despliegue/orquestación de múltiples BD por departamento (migraciones, backups)
- [x] Framework de frontend adicional — resuelto: Vite + React 19 + TypeScript, React Router para navegación, Context API para estado (sin librería adicional por ahora), sistema de diseño propio en CSS (sin UI kit externo). Ver `frontend/`.

---

## Historial de cambios

| Versión | Fecha | Autor | Cambios |
|---------|-------|-------|---------|
| 1.0 | 2026-08-15 | - | Documento inicial, decisiones de stack técnico |
