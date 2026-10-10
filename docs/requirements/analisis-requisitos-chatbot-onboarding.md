# Análisis de Requisitos: Plataforma de Chatbots de Onboarding

**Fecha:** 2026-07-24
**Versión:** 1.0
**Estado:** Borrador inicial

---

## 1. Contexto del proyecto

Empresa tech con alta rotación/incorporación de desarrolladores. Los programadores senior dedican tiempo significativo a responder preguntas repetitivas. Cada departamento tiene su propia documentación, convenciones y conocimiento tribal disperso.

**Situación actual:**
- Documentación fragmentada (Confluence, Notion, READMEs, Slack, cabezas de seniors)
- Nuevos desarrolladores usan ChatGPT/Copilot y aplican código sin entender contexto interno
- Curva de onboarding larga (típicamente 3-6 meses para productividad plena)

**Propuesta de solución:**
Aplicación para gestionar y crear chatbots nutridos con documentos o información específica de cada departamento. Cada departamento puede crear su propio chat con conocimiento propio para ayudar a los nuevos desarrolladores a aprender, no solo a copiar código.

---

## 2. Problema / Necesidad a resolver

| Problema | Impacto |
|----------|---------|
| IA genérica sin contexto de empresa | Código que no sigue convenciones internas, arquitectura inconsistente |
| Conocimiento tribal no documentado | Dependencia de seniors, bottleneck de productividad |
| Documentación dispersa | Tiempo perdido buscando información |
| Copiar código sin entender | Deuda técnica, bugs difíciles de debuggear |
| Seniors interrumpidos constantemente | Pérdida de foco, frustración |

**Problema raíz:** Los nuevos desarrolladores necesitan **aprender**, no solo obtener respuestas.

---

## 3. Investigación competitiva y alternativas

### 3.1 Análisis de competidores

| Solución | Funcionalidades clave | Ventajas | Limitaciones |
|----------|----------------------|----------|--------------|
| **Glean** | Búsqueda enterprise cross-platform, RAG sobre 20+ integraciones | Potente en empresas grandes, respeta permisos | Mínimo 100 usuarios, $50K+/año, no enfocado en aprendizaje |
| **Guru** | Knowledge base verificada, cards de conocimiento | Sistema de verificación, bueno para soporte | No es educativo, más orientado a sales/support |
| **Notion AI** | Q&A sobre workspace Notion | Barato si ya usas Notion | Solo funciona sobre Notion, no multi-fuente |
| **Slite** | Docs + Q&A simple, $8/usuario | Económico, fácil adopción | Limitado en capacidades técnicas |
| **CustomGPT.ai** | No-code RAG chatbot | Rápido de desplegar, cita fuentes | Genérico, sin enfoque educativo |

### 3.2 Gap del mercado identificado

Ninguna solución está diseñada específicamente para:
- **Enseñar** en lugar de solo responder
- Contexto de **desarrollo de software** (repos, PRs, arquitectura)
- **Multi-tenant por departamento** con conocimiento aislado

### 3.3 Fuentes de investigación

- [Best AI Knowledge Management Tools 2026](https://effectiveaitools.com/posts/best-ai-knowledge-management-tools-2026/)
- [Guru vs Glean Comparison](https://coworker.ai/blog/guru-vs-glean)
- [Glean vs Guru vs Notion Comparison](https://slashdot.org/software/comparison/Glean-Work-AI-vs-Guru-vs-Notion/)
- [AI Knowledge Management Tools](https://happysupport.ai/blog/ai-knowledge-management-tools)

---

## 4. Insights de investigación usuario/negocio

### 4.1 Perfiles a entrevistar (recomendados)

| Perfil | Por qué entrevistarlos | Preguntas clave |
|--------|------------------------|-----------------|
| Junior recién incorporado (0-3 meses) | Experimenta el problema directamente | ¿Dónde buscas cuando no sabes algo? ¿Qué te frustra? |
| Junior con 6-12 meses | Ya pasó el onboarding, recuerda pains | ¿Qué te hubiera ayudado al principio? |
| Senior/Tech Lead | Responde preguntas repetitivas | ¿Qué preguntas recibes constantemente? ¿Cuánto tiempo dedicas? |
| Engineering Manager | Visión de productividad del equipo | ¿Cómo mides el tiempo de onboarding? |
| DevOps/Infra | Conocimiento crítico y poco documentado | ¿Qué conocimiento está solo en tu cabeza? |

### 4.2 Hipótesis de necesidades

| Necesidades Explícitas | Necesidades Implícitas |
|------------------------|------------------------|
| "Necesito encontrar cómo funciona X" | Quiero sentirme competente sin molestar |
| "La documentación está desactualizada" | No sé qué no sé |
| "ChatGPT no conoce nuestro código" | Miedo a parecer incompetente |

---

## 5. Requisitos funcionales

### 5.1 Core (MVP)

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| RF1 | Gestión de chatbots por departamento | CRUD de chatbots, cada departamento administra el suyo | Alta |
| RF2 | Ingesta de documentos | Upload de PDFs, Markdown, URLs, Confluence, Notion | Alta |
| RF3 | Chat conversacional con RAG | Respuestas basadas en documentos ingestados | Alta |
| RF4 | Citación de fuentes | Toda respuesta muestra de dónde viene la información | Alta |
| RF5 | Control de acceso | Usuarios solo ven chatbots de sus departamentos | Alta |
| RF6 | Panel de administración | Gestionar usuarios, documentos, configuración de bots | Alta |
| RF19 | ~~Chatbot general de empresa~~ (descartado) | Descartado en la versión 1.3: no aporta nada que un chatbot normal no cubra. Un chatbot de RRHH o de IT con su propia documentación resuelve esas dudas con la misma mecánica (RF1-RF9), sin un tipo de chatbot aparte | - |

### 5.2 Funcionalidades educativas (Diferenciador)

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| RF7 | Modo "Explícame más" | Respuestas en capas de profundidad adaptable | Alta |
| RF8 | Botón "¿Por qué así?" | Explica decisiones de arquitectura, no solo el cómo | Alta |
| RF9 | Modo Socrático | Hace preguntas guiadas para que el junior llegue solo a la respuesta | Media |
| RF10 | Quiz de comprensión | Valida que el usuario entendió, no solo copió (absorbido y ampliado por RF23) | Media |
| RF23 | Currículo de formación | Entidad independiente de los chatbots — no es una pestaña de un chatbot, sino su propia sección ("Formación") con listado y gestión propios. Un currículo tiene una audiencia (p. ej. un departamento o "toda la empresa") y, opcionalmente, un chatbot de referencia usado solo como fuente de contexto para generar contenido por IA, sin que el currículo dependa de él. Se compone de módulos tipados — vídeo, documento, ejercicio o examen —, cada uno con sus propios campos y proceso de realización (ver un vídeo, leer un documento, abrir un ejercicio, o responder un examen con preguntas de opción múltiple y nota mínima). El admin configura por módulo si bloquea el avance al siguiente (gating secuencial) o es de recorrido libre. El contenido de examen se genera como borrador asistido por IA a partir de los documentos indexados del chatbot de referencia, y el admin lo revisa antes de publicarlo. El progreso de cada persona es visible en el propio currículo. Da estructura y visualización al onboarding, ampliando RF10 y RF17 | Alta |

### 5.3 Funcionalidades de productividad

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| RF11 | Contexto de repo | Conecta con GitHub/GitLab, responde sobre código real | Media |
| RF12 | Snippets verificados | Código aprobado por seniors, no generado | Media |
| RF13 | Detección de gaps | Analytics de preguntas sin buena respuesta | Media |
| RF14 | Escalado a humano | Conecta con el senior apropiado si el bot no puede | Baja |
| RF20 | Análisis de preguntas más repetidas | Distinto de RF13 (que detecta preguntas *sin buena respuesta*): analiza la frecuencia/recurrencia de preguntas independientemente de si la respuesta fue buena. Permite al administrador del chatbot identificar las dudas más repetitivas de su equipo y plantear un plan para solventarlas (nutrir el bot con más contenido, documentar puntos no cubiertos, etc.) | Media |
| RF21 | Gobernanza de coste/uso de LLM por departamento | Cuotas y alertas de gasto por chatbot/departamento, dado que cada bot puede configurar su propio proveedor de LLM (RF1). Da visibilidad de coste a quien aprueba presupuesto | Media |

### 5.4 Funcionalidades de cultura y comunidad

| ID | Requisito | Descripción | Prioridad |
|----|-----------|-------------|-----------|
| RF15 | Leaderboard de contribuciones | Gamifica documentación y mejora de respuestas | Baja |
| RF16 | Historial para seniors | Ver qué preguntan los juniors | Baja |
| RF17 | Onboarding paths | Rutas guiadas por semanas/temas | Baja |
| RF18 | Feedback bidireccional | Juniors valoran, seniors verifican | Media |
| RF22 | Encuestas estructuradas de satisfacción (eNPS) | En hitos fijos (semana 1, mes 1, mes 3), a diferencia del feedback puntual por mensaje (RF18); mide la experiencia global del proceso de onboarding, no solo respuestas individuales | Media |

---

## 6. Requisitos no funcionales

| ID | Categoría | Requisito | Métrica |
|----|-----------|-----------|---------|
| RNF1 | Seguridad | Datos aislados por tenant (departamento) | 0 cruces de información |
| RNF2 | Seguridad | Autenticación SSO (SAML/OIDC) | Integración con directorio corporativo |
| RNF3 | Rendimiento | Tiempo de respuesta | <3 segundos para consultas típicas |
| RNF4 | Escalabilidad | Consultas concurrentes | 50 por departamento |
| RNF5 | Disponibilidad | Uptime | 99.5% en horario laboral |
| RNF6 | Privacidad | Despliegue | Opción on-premise o cloud privado |
| RNF7 | UX | Fricción | Interfaz minimalista, cero barreras |
| RNF8 | Mantenibilidad | Actualización de docs | Sin downtime |

---

## 7. Restricciones, dependencias y riesgos

### 7.1 Restricciones

- Presupuesto de LLM (tokens) puede escalar rápido
- Calidad del chatbot depende de calidad de documentación existente
- Requiere buy-in de cada departamento para nutrir el bot

### 7.2 Dependencias técnicas

| Componente | Opciones | Consideraciones |
|------------|----------|-----------------|
| LLM | OpenAI, Anthropic, modelo local (Llama, Mistral) | Costo vs privacidad vs calidad |
| Vector DB | Pinecone, Weaviate, Qdrant, Chroma | Escalabilidad, costo, self-hosted |
| Integraciones | Confluence, Notion, GitHub, Slack | APIs disponibles |

### 7.3 Riesgos

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|--------------|---------|------------|
| Baja adopción por seniors | Media | Alto | Mostrar valor (menos interrupciones), métricas |
| Respuestas incorrectas/alucinaciones | Media | Alto | Citación obligatoria, feedback loop, verificación |
| Documentación pobre = bot inútil | Alta | Alto | Analytics de gaps, incentivos para documentar |
| Costos de LLM descontrolados | Media | Medio | Caching, modelos locales, rate limiting |
| Resistencia al cambio | Media | Medio | Onboarding gradual, champions por departamento |

---

## 8. Hipótesis a validar

| ID | Hipótesis | Método de validación | Criterio de éxito |
|----|-----------|---------------------|-------------------|
| H1 | Los juniors prefieren preguntar a un bot antes que a un senior | Encuesta + métricas de uso | >60% prefieren bot para preguntas iniciales |
| H2 | El tiempo de onboarding se reduce con acceso a knowledge bot | Comparar cohortes | Reducción ≥20% |
| H3 | Los seniors recuperan tiempo si las preguntas repetitivas van al bot | Time tracking | >2 horas/semana recuperadas |
| H4 | Los departamentos están dispuestos a invertir tiempo en nutrir bots | Piloto con 2-3 departamentos | >50 docs ingestados en 1 mes |
| H5 | Mostrar el "por qué" reduce errores de implementación | Análisis de PRs | Menos comentarios de revisión |

---

## 9. Valor esperado y métricas de impacto

### 9.1 Valor para la empresa

- Reducción de tiempo de onboarding
- Liberación de tiempo de seniors
- Captura de conocimiento tribal
- Estandarización de prácticas

### 9.2 Valor para los usuarios

**Juniors:**
- Autonomía para resolver problemas
- Aprendizaje real, no solo copiar código
- Menor fricción social al preguntar

**Seniors:**
- Menos interrupciones
- Preguntas que llegan son más complejas/interesantes
- Contribución escalable al conocimiento

### 9.3 KPIs sugeridos

| Métrica | Baseline actual | Target | Cómo medir |
|---------|-----------------|--------|------------|
| Tiempo hasta productividad | 90 días | 60 días | Fecha de primer PR productivo |
| Preguntas directas a seniors/semana | ~15 | <5 | Encuesta semanal |
| Satisfacción onboarding (NPS) | +20 | +50 | Survey a los 30/60/90 días |
| % respuestas útiles del bot | - | >80% | Feedback thumbs up/down |
| Docs ingestados por departamento | 0 | >100 | Contador en sistema |
| Usuarios activos semanales | - | >70% de juniors | Analytics |

### 9.4 Modelo de valor/monetización (si aplica)

- **Interno:** Ahorro en tiempo de seniors, reducción de rotación temprana
- **Producto externo (futuro):** SaaS por seats, tier enterprise con SSO/compliance

---

## 10. Oportunidades futuras e innovaciones

### 10.1 Corto plazo (6 meses)

- Integración con IDE (extensión VS Code) para respuestas en contexto
- Slack bot para preguntas rápidas
- Dashboard de analytics para managers

### 10.2 Medio plazo (12 meses)

- Generación automática de documentación desde PRs/commits
- Detección proactiva: "Veo que estás en X archivo, ¿necesitas saber sobre Y?"
- Personalización por nivel del desarrollador

### 10.3 Largo plazo (18+ meses)

- Agente que puede ejecutar acciones (crear tickets, configurar entornos)
- Análisis de código en tiempo real con sugerencias contextualizadas
- Marketplace de conocimiento entre empresas (anonimizado)
- Certificaciones internas basadas en interacciones con el bot

---

## 11. Propuesta de MVP

```
┌─────────────────────────────────────────────────────────┐
│                      MVP (3-4 meses)                    │
├─────────────────────────────────────────────────────────┤
│  ✓ RF1: CRUD chatbots por departamento                  │
│  ✓ RF2: Ingesta de docs (MD, PDF, URLs)                 │
│  ✓ RF3: Chat con RAG + citación de fuentes              │
│  ✓ RF4: Citación obligatoria de fuentes                 │
│  ✓ RF5: Control de acceso básico                        │
│  ✓ RF7: Modo "Explícame más" (profundidad adaptable)    │
│  ✓ RF8: Botón "¿Por qué así?" en respuestas de código   │
└─────────────────────────────────────────────────────────┘
```

---

## 12. Próximos pasos recomendados

1. [x] Validar hipótesis con 3-5 entrevistas (juniors + seniors)
2. [x] Definir stack técnico (LLM, vector DB, framework)
3. [x] Prototipo de conversación (mockup de UX)
4. [ ] Piloto con 1-2 departamentos
5. [ ] Definir métricas de éxito para el piloto

---

## 13. Funcionalidades candidatas adicionales (pendientes de evaluar)

Ideas de valor identificadas en revisión posterior, relacionadas con el ámbito de la plataforma de onboarding pero no necesariamente ligadas al chatbot en sí. No forman parte del alcance decidido todavía — se listan para su evaluación y priorización futura.

| Candidata | Descripción |
|-----------|-------------|
| Checklist de onboarding administrativo integrado | Tareas del proceso de incorporación que no son preguntas al bot (alta de accesos, documentos a firmar, asignación de equipo/hardware), centralizadas en la misma plataforma junto a los chatbots |
| Directorio de mentores/"buddy" por departamento | Referente humano visible desde la plataforma para cada departamento, para los casos en que el bot no sea suficiente o el matiz humano importe; complementa al chatbot, no lo sustituye |
| Panel de salud documental a nivel de plataforma | A diferencia de la analítica por bot (RF13/RF20), un panel transversal que muestre qué departamentos tienen documentación más escasa o desactualizada, para priorizar esfuerzo de documentación de forma comparativa entre equipos |
| Integración con el sistema de RRHH (HRIS) | Alta automática de usuario y asignación de departamento/permisos cuando RRHH da de alta a un empleado, evitando gestión manual de accesos el primer día |
| Plantillas de onboarding reutilizables por rol/departamento | Checklist + recursos + cronograma predefinidos para que RRHH pueda montar rápidamente el proceso de un perfil nuevo sin partir de cero cada vez |

---

## Historial de cambios

| Versión | Fecha | Autor | Cambios |
|---------|-------|-------|---------|
| 1.0 | 2026-07-24 | - | Documento inicial |
| 1.1 | 2026-08-18 | - | Añadidos RF19 (chatbot general de empresa), RF20 (análisis de preguntas más repetidas), RF21 (gobernanza de coste/uso de LLM), RF22 (encuestas eNPS) y RF23 (currículo de formación). Añadida sección 13 con candidatas adicionales pendientes de evaluar. Marcados como completados los pasos 1-3 de la sección 12 |
| 1.2 | 2026-08-18 | - | RF23 rediseñado como entidad independiente de los chatbots (sección "Formación" propia, con listado y gestión propios), en vez de una pestaña dentro de cada chatbot. Los módulos pasan a ser tipados (vídeo, documento, ejercicio, examen), cada uno con su propio proceso de realización |
| 1.3 | 2026-10-10 | - | RF19 (chatbot general de empresa) descartado: un chatbot normal de RRHH o de IT cubre el mismo caso sin un tipo de chatbot aparte. Se elimina el flag "general" del producto |