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

  RateExchange(botKey: string, conversationId: string, exchangeIndex: number, useful: boolean): Promise<void>;

  Delete(conversationId: string): Promise<void>;
}
