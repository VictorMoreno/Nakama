import type { ConversationSummary } from "./Chatbot";

export interface IConversationsRepository {
  FindAll(userId: string): Promise<ConversationSummary[]>;

  AppendExchange(
    conversationId: string,
    botKey: string,
    userId: string,
    title: string,
    question: string,
    answer: string,
    citation: string,
    moreExplanation: string,
    socraticVariant: string
  ): Promise<void>;

  Delete(botKey: string, conversationId: string, userId: string): Promise<void>;
}
