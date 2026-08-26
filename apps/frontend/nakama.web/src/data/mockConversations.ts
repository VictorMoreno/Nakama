import type { ConversationSummary } from "../types/chatbot";
import { mockChatbots } from "./mockChatbots";

/**
 * Placeholder past conversations for the chat history panel. Only chatbots with a
 * non-empty `lastUsed` in mockChatbots have entries here — bots never opened yet
 * intentionally have no history group.
 *
 * Deliberately built from `chips`, never from `opening`: `opening` is what the chat
 * panel already shows by default when you select a bot, so reusing it here would make
 * a saved conversation share its first exchange with the live chat — the two would
 * look like one merged thread instead of distinct, non-overlapping conversations.
 */
export const mockConversations: ConversationSummary[] = [
  {
    id: "conv-ecu-1",
    botKey: "ecu",
    title: mockChatbots.ecu.chips[0].q,
    dateLabel: "Hoy",
    exchanges: [mockChatbots.ecu.chips[0]]
  },
  {
    id: "conv-auth-1",
    botKey: "auth",
    title: mockChatbots.auth.chips[0].q,
    dateLabel: "Hace 3 días",
    exchanges: [mockChatbots.auth.chips[0]]
  }
];
