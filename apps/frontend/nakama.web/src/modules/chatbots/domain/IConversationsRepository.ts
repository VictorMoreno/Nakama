import type { ConversationSummary } from "./Chatbot";

export interface IConversationsRepository {
  FindAll(): Promise<ConversationSummary[]>;

  Ask(
    conversationId: string,
    botKey: string,
    title: string,
    question: string,
    files: File[],
    onChunk: (textSoFar: string) => void
  ): Promise<string>;

  Delete(conversationId: string): Promise<void>;
}
