import type { IConversationsRepository } from "../domain/IConversationsRepository";
import type { ConversationSummary } from "../domain/Chatbot";

/**
 * Deliberately built from each bot's own follow-up question, never from its `opening`:
 * `opening` is what the chat panel already shows by default when you select a bot, so
 * reusing it here would make a saved conversation share its first exchange with the live
 * chat — the two would look like one merged thread instead of distinct, non-overlapping
 * conversations. Only chatbots with a non-empty `lastUsed` have entries here — bots never
 * opened yet intentionally have no history.
 */
const seedConversations: ConversationSummary[] = [
  {
    id: "conv-ecu-1",
    botKey: "ecu",
    title: "¿Dónde configuro el timeout de conexión con la ECU?",
    dateLabel: "Hoy",
    exchanges: [
      {
        q: "¿Dónde configuro el timeout de conexión con la ECU?",
        a: "El timeout de conexión se define en <code>appsettings.json</code>, bajo <code>EcuProtocols:ConnectTimeoutMs</code> (por defecto 3000 ms). Cada adaptador puede sobrescribirlo — Volvo, por ejemplo, necesita 5000 ms por el arranque en frío del bus CAN.",
        cite: "Config-Reference.md §Protocolos",
        more: "Este valor solo afecta al <em>connect</em>, no a la lectura de códigos: <code>ReadFaultCodes()</code> tiene su propio <code>ReadTimeoutMs</code>, porque los buses CAN antiguos responden más lento bajo carga.",
        socratic:
          "¿Qué pasaría si el mismo timeout se usara tanto para conectar como para leer códigos de avería? Si un bus CAN antiguo responde despacio bajo carga, ¿a qué fase crees que afectaría más cortar la conexión demasiado pronto?"
      }
    ]
  },
  {
    id: "conv-auth-1",
    botKey: "auth",
    title: "¿Qué pasa si un taller pierde una tablet?",
    dateLabel: "Hace 3 días",
    exchanges: [
      {
        q: "¿Qué pasa si un taller pierde una tablet?",
        a: "El token de esa sesión se revoca desde el panel de Jaltest Auth. No hace falta rotar ningún secreto, porque no hay ninguno compartido embebido en el cliente — solo se invalida la sesión de esa tablet concreta.",
        cite: "Seguridad-Auth-Talleres.md §4",
        more: "La revocación corta el <em>refresh token</em> asociado a esa sesión; la próxima vez que la tablet intente renovar su acceso, Jaltest Auth la rechaza y exige un login nuevo.",
        socratic:
          "Con el modelo antiguo de secreto compartido, perder una tablet obligaba a rotar el secreto para todas. Con PKCE, ¿qué crees que hace falta revocar cuando se pierde una tablet?"
      }
    ]
  }
];

export class MockConversationsRepository implements IConversationsRepository {
  async FindAll(): Promise<ConversationSummary[]> {
    return structuredClone(seedConversations);
  }
}
