import type { ConversationSummary } from "./Chatbot";

export interface IConversationsRepository {
  FindAll(): Promise<ConversationSummary[]>;
}
