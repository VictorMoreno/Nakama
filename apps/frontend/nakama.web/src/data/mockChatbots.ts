import type { ChatbotMap } from "../types/chatbot";

/**
 * Placeholder data standing in for the real backend (RF1-RF3). Shaped exactly like the
 * API contract we expect from the .NET side so swapping this for a real fetch later is
 * a data-layer change only — components should not need to change.
 */
export const mockChatbots: ChatbotMap = {
  ecu: {
    key: "ecu",
    label: "Diagnósticos ECU",
    description: "Protocolos de comunicación con la ECU, arquitectura de adaptadores y decisiones de diseño del equipo.",
    status: "atencion",
    lastUsed: "Usaste este chatbot hoy",
    context:
      "Nutrido con la documentación técnica del equipo de <strong>Diagnósticos ECU</strong>: arquitectura de protocolos, referencias de configuración y decisiones de diseño registradas por los seniors del equipo.",
    opening: {
      q: "¿Cómo añado un protocolo nuevo de comunicación con la ECU sin romper los que ya existen?",
      a: "Los protocolos se implementan como adaptadores del puerto <code>IEcuProtocol</code>. Para añadir uno nuevo:<ol><li>Crea una clase que implemente <code>IEcuProtocol</code> en <code>Infrastructure/Protocols/</code>.</li><li>Regístrala en el <code>ProtocolFactory</code> con el identificador del fabricante.</li><li>No toques <code>EcuReader</code> — ya no depende de protocolos concretos.</li></ol>",
      cite: "Arquitectura-Protocolos-ECU.md §3.2",
      more: "El puerto <code>IEcuProtocol</code> define <code>Connect()</code>, <code>ReadFaultCodes()</code> y <code>Disconnect()</code>. <code>EcuReader</code> solo conoce esa interfaz, nunca las clases concretas — así, añadir Volvo o Scania nuevos no obliga a tocar el código que ya funciona con MAN o DAF.",
      socratic:
        "Antes de darte la respuesta: si añadieras el protocolo directamente dentro de la clase <code>EcuReader</code> que ya existe, ¿qué tendrías que tocar cada vez que llega un fabricante nuevo? ¿Y si ese cambio rompe la lectura de un fabricante que ya funciona en producción?"
    },
    chips: [
      {
        q: "¿Dónde configuro el timeout de conexión con la ECU?",
        a: "El timeout de conexión se define en <code>appsettings.json</code>, bajo <code>EcuProtocols:ConnectTimeoutMs</code> (por defecto 3000 ms). Cada adaptador puede sobrescribirlo — Volvo, por ejemplo, necesita 5000 ms por el arranque en frío del bus CAN.",
        cite: "Config-Reference.md §Protocolos",
        more: "Este valor solo afecta al <em>connect</em>, no a la lectura de códigos: <code>ReadFaultCodes()</code> tiene su propio <code>ReadTimeoutMs</code>, porque los buses CAN antiguos responden más lento bajo carga.",
        socratic:
          "¿Qué pasaría si el mismo timeout se usara tanto para conectar como para leer códigos de avería? Si un bus CAN antiguo responde despacio bajo carga, ¿a qué fase crees que afectaría más cortar la conexión demasiado pronto?"
      }
    ],
    settings: { socratic: false, explicaMas: true, provider: "anthropic" },
    docs: [
      { name: "Arquitectura-Protocolos-ECU.md", origin: "Confluence", status: "ok", statusLabel: "Indexado", updated: "hace 2 días" },
      { name: "Config-Reference.md", origin: "Repositorio", status: "ok", statusLabel: "Indexado", updated: "hace 5 días" },
      { name: "Manual-ECU-Volvo-2024.pdf", origin: "Subida manual", status: "pending", statusLabel: "Procesando", updated: "hace 10 min" }
    ],
    gaps: [{ q: "¿Cómo depuro un fallo intermitente en el bus CAN de un DAF?", count: 4 }],
    topQuestions: [
      { q: "¿Dónde configuro el timeout de conexión con la ECU?", count: 11 },
      { q: "¿Cómo añado un protocolo nuevo de comunicación con la ECU?", count: 9 },
      { q: "¿Qué diferencia hay entre ReadFaultCodes y Connect?", count: 6 }
    ],
    team: [
      { name: "Marta Vidal", role: "Propietaria" },
      { name: "Jon Etxeberria", role: "Editor" },
      { name: "Iker Sola", role: "Lector" }
    ],
    stats: [
      { label: "Tiempo hasta productividad", value: "72", unit: "días · objetivo 60", bar: 62 },
      { label: "Preguntas directas a seniors / semana", value: "8", unit: "objetivo <5", bar: 55 },
      { label: "Respuestas útiles del bot", value: "76", unit: "% · objetivo 80%", bar: 76 },
      { label: "Documentos ingestados", value: "3", unit: "objetivo >100", bar: 3 }
    ]
  },

  auth: {
    key: "auth",
    label: "Acceso Talleres",
    description: "Autenticación PKCE, gestión de sesiones y políticas de acceso para las tablets de taller.",
    status: "atencion",
    lastUsed: "Usaste este chatbot hace 3 días",
    context:
      "Nutrido con la documentación del equipo de <strong>Acceso Talleres</strong>: flujo de autenticación con Jaltest Auth, gestión de sesiones y políticas de acceso para las tablets de taller.",
    opening: {
      q: "¿Por qué el login de la app de talleres usa PKCE en vez de un client secret?",
      a: "La app de talleres es un cliente <strong>público</strong> — se instala en tablets de taller, no vive en un servidor. Un <code>client_secret</code> embebido en una app pública se puede extraer y usar para suplantarla. PKCE genera un secreto de un solo uso por cada login: aunque alguien intercepte el <code>authorization code</code>, no puede canjearlo sin el <code>code_verifier</code> original.",
      cite: "Seguridad-Auth-Talleres.md §1",
      more: "El flujo: la app genera un <code>code_verifier</code> aleatorio, calcula su hash (<code>code_challenge</code>) y lo envía al iniciar el login. Jaltest Auth guarda ese hash. Al canjear el código por el token, la app reenvía el <code>code_verifier</code> original — solo coincide si es la misma app que empezó el login.",
      socratic:
        "Si todas las tablets de taller compartieran el mismo <code>client_secret</code> embebido en la app, ¿qué pasaría si una sola tablet se pierde o alguien descompila el binario? ¿Cuántas tablets se verían afectadas?"
    },
    chips: [
      {
        q: "¿Qué pasa si un taller pierde una tablet?",
        a: "El token de esa sesión se revoca desde el panel de Jaltest Auth. No hace falta rotar ningún secreto, porque no hay ninguno compartido embebido en el cliente — solo se invalida la sesión de esa tablet concreta.",
        cite: "Seguridad-Auth-Talleres.md §4",
        more: "La revocación corta el <em>refresh token</em> asociado a esa sesión; la próxima vez que la tablet intente renovar su acceso, Jaltest Auth la rechaza y exige un login nuevo.",
        socratic:
          "Con el modelo antiguo de secreto compartido, perder una tablet obligaba a rotar el secreto para todas. Con PKCE, ¿qué crees que hace falta revocar cuando se pierde una tablet?"
      }
    ],
    settings: { socratic: true, explicaMas: true, provider: "openai" },
    docs: [
      { name: "Seguridad-Auth-Talleres.md", origin: "Confluence", status: "ok", statusLabel: "Indexado", updated: "hace 1 semana" },
      { name: "Guía-Onboarding-Talleres.pdf", origin: "Subida manual", status: "pending", statusLabel: "Pendiente", updated: "hace 3 días" }
    ],
    gaps: [{ q: "¿Cómo doy de alta un taller nuevo en Jaltest Auth?", count: 2 }],
    topQuestions: [
      { q: "¿Qué pasa si un taller pierde una tablet?", count: 7 },
      { q: "¿Por qué el login usa PKCE en vez de client secret?", count: 5 }
    ],
    team: [
      { name: "Sara Molina", role: "Propietaria" },
      { name: "David Prats", role: "Lector" }
    ],
    stats: [
      { label: "Tiempo hasta productividad", value: "68", unit: "días · objetivo 60", bar: 78 },
      { label: "Preguntas directas a seniors / semana", value: "6", unit: "objetivo <5", bar: 80 },
      { label: "Respuestas útiles del bot", value: "81", unit: "% · objetivo 80%", bar: 100 },
      { label: "Documentos ingestados", value: "2", unit: "objetivo >100", bar: 2 }
    ]
  },

  cloud: {
    key: "cloud",
    label: "Plataforma Cloud",
    description: "Arquitectura multi-departamento, stack técnico y decisiones de infraestructura compartida.",
    status: "pausado",
    lastUsed: "Aún no has usado este chatbot",
    context:
      "Nutrido con la documentación de <strong>Plataforma Cloud</strong>: arquitectura multi-departamento, stack técnico y decisiones de infraestructura compartida.",
    opening: {
      q: "¿Por qué cada departamento tiene su propia base de datos en vez de compartir una?",
      a: "Es aislamiento por diseño: cada departamento (Diagnósticos, Talleres, Plataforma…) tiene su propia base PostgreSQL. Así, un fallo de permisos o un bug de filtrado en un departamento no puede exponer datos de otro.",
      cite: "Stack-Tecnico.md §2",
      more: "La resolución de qué base usar ocurre en la capa de infraestructura, antes de llegar a la lógica de negocio — el dominio nunca ve una cadena de conexión ni sabe que existen otros departamentos.",
      socratic:
        "Si todos los departamentos compartieran una base con una columna <code>department_id</code>, ¿qué pasaría el día que alguien olvide un <code>WHERE department_id = ?</code> en una consulta nueva?"
    },
    chips: [
      {
        q: "¿Qué pasa si pgvector no rinde con muchos documentos?",
        a: "El acceso a los vectores pasa por un puerto (<code>IVectorStorePort</code>). Si pgvector deja de rendir a cierta escala, se sustituye la implementación por Qdrant u otra alternativa sin tocar el resto de la aplicación.",
        cite: "Stack-Tecnico.md §1",
        more: "El puerto expone operaciones como <code>Search()</code> e <code>Upsert()</code> en términos de dominio (embeddings, similitud), no en términos de PostgreSQL — el adaptador concreto es un detalle de infraestructura intercambiable.",
        socratic:
          "Si el código de negocio llamara directamente a funciones específicas de pgvector en vez de a un puerto genérico, ¿qué tendría que reescribirse el día que hiciera falta cambiar a otra base vectorial?"
      }
    ],
    settings: { socratic: false, explicaMas: true, provider: "local" },
    docs: [{ name: "Stack-Tecnico.md", origin: "Repositorio", status: "ok", statusLabel: "Indexado", updated: "hoy" }],
    gaps: [{ q: "¿Qué pasa si dos departamentos necesitan el mismo documento?", count: 1 }],
    topQuestions: [{ q: "¿Por qué cada departamento tiene su propia base de datos?", count: 8 }],
    team: [
      { name: "Pau Ferran", role: "Propietario" },
      { name: "Marta Vidal", role: "Editor" }
    ],
    stats: [
      { label: "Tiempo hasta productividad", value: "75", unit: "días · objetivo 60", bar: 52 },
      { label: "Preguntas directas a seniors / semana", value: "9", unit: "objetivo <5", bar: 48 },
      { label: "Respuestas útiles del bot", value: "70", unit: "% · objetivo 80%", bar: 70 },
      { label: "Documentos ingestados", value: "1", unit: "objetivo >100", bar: 1 }
    ]
  },

  general: {
    key: "general",
    label: "General de Empresa",
    description: "Dudas transversales de empresa: políticas internas, RRHH, IT y herramientas comunes a todos los departamentos.",
    status: "activo",
    lastUsed: "Aún no has usado este chatbot",
    context:
      "Chatbot transversal, con la misma mecánica que los chatbots de departamento, pero nutrido con información <strong>general de la empresa</strong> (políticas internas, herramientas comunes, RRHH, IT) en vez de documentación de un equipo técnico concreto. Resuelve las dudas que no pertenecen a ningún departamento.",
    opening: {
      q: "¿Cómo solicito acceso VPN para trabajar en remoto?",
      a: "El acceso VPN se solicita desde el portal de IT interno, seleccionando 'Alta de acceso remoto'. Tu manager recibe la aprobación automáticamente y sueles tener acceso en menos de 24 horas.",
      cite: "Manual-IT-Empresa.md §Accesos remotos",
      more: "La VPN corporativa usa autenticación con el mismo usuario de Jaltest Auth que el resto de herramientas — no hace falta crear una contraseña nueva.",
      socratic: "¿Dónde crees que se gestionan el resto de accesos de la empresa (correo, calendario, herramientas)? ¿Por qué tendría sentido que la VPN siguiera el mismo camino?"
    },
    chips: [
      {
        q: "¿Dónde consulto la política de vacaciones?",
        a: "La política de vacaciones está en el portal de RRHH, dentro de 'Políticas internas'. Cada departamento puede tener acuerdos particulares de flexibilidad, pero el mínimo legal y el proceso de solicitud son comunes a toda la empresa.",
        cite: "Politicas-RRHH.md §Vacaciones",
        more: "Las solicitudes se aprueban por tu manager directo desde el mismo portal; el histórico de días usados también es visible ahí.",
        socratic: "Si tu departamento tuviera una política distinta a la general, ¿dónde esperarías que estuviera documentada esa excepción?"
      }
    ],
    settings: { socratic: false, explicaMas: true, provider: "anthropic" },
    docs: [
      { name: "Manual-IT-Empresa.md", origin: "Confluence", status: "ok", statusLabel: "Indexado", updated: "hace 3 días" },
      { name: "Politicas-RRHH.md", origin: "Confluence", status: "ok", statusLabel: "Indexado", updated: "hace 1 semana" }
    ],
    gaps: [{ q: "¿Cómo reservo una sala de reuniones en la sede de Bilbao?", count: 3 }],
    topQuestions: [
      { q: "¿Cómo solicito acceso VPN para trabajar en remoto?", count: 14 },
      { q: "¿Dónde consulto la política de vacaciones?", count: 10 }
    ],
    team: [
      { name: "RRHH — Recursos Humanos", role: "Propietaria" },
      { name: "Iker Sola", role: "Lector" }
    ],
    stats: [
      { label: "Tiempo hasta productividad", value: "—", unit: "no aplica (transversal)", bar: 0 },
      { label: "Preguntas directas a seniors / semana", value: "—", unit: "no aplica (transversal)", bar: 0 },
      { label: "Respuestas útiles del bot", value: "84", unit: "% · objetivo 80%", bar: 84 },
      { label: "Documentos ingestados", value: "2", unit: "objetivo >100", bar: 2 }
    ],
    general: true
  }
};

export const currentUser = {
  name: "Iker Sola",
  roleLine: "Lector · Diagnósticos ECU"
};
