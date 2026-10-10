import type { ConversationSummary } from "./Chatbot";

export interface IConversationsRepository {
  FindAll(userId: string): Promise<ConversationSummary[]>;

  Ask(
    conversationId: string,
    botKey: string,
    userId: string,
    title: string,
    question: string,
    files: File[],
    onChunk: (textSoFar: string) => void
  ): Promise<string>;

  Delete(conversationId: string, userId: string): Promise<void>;
}
