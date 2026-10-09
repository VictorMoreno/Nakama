import type { ConversationSummary } from "./Chatbot";

export interface IConversationsRepository {
  FindAll(userId: string): Promise<ConversationSummary[]>;

  Ask(
    conversationId: string,
    botKey: string,
    userId: string,
    title: string,
    question: string,
    onChunk: (textSoFar: string) => void
  ): Promise<string>;

  Delete(botKey: string, conversationId: string, userId: string): Promise<void>;
}
