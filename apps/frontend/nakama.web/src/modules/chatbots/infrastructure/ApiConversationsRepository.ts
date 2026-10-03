import type { GeneratedNakamaClient } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { ChatExample, ConversationSummary } from "../domain/Chatbot";
import type { IConversationsRepository } from "../domain/IConversationsRepository";

function formatDate(value: Date | string | undefined): string {
  if (!value) return "";
  const date = typeof value === "string" ? new Date(value) : value;
  return Number.isNaN(date.getTime()) ? "" : date.toLocaleDateString("es-ES");
}

function toChatExample(row: { question?: string; answer?: string; citation?: string; moreExplanation?: string; socraticVariant?: string }): ChatExample {
  return { q: row.question ?? "", a: row.answer ?? "", cite: row.citation ?? "", more: row.moreExplanation ?? "", socratic: row.socraticVariant ?? "" };
}

/**
 * No backend endpoint returns "every conversation across every chatbot" directly - history
 * lives in its own per-bot projection (conversationHistory(botKey, userId).entries), so this
 * fans out over chatbotsAll()+conversationHistory(botKey, userId) per bot and flattens the result.
 */
export class ApiConversationsRepository implements IConversationsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(userId: string): Promise<ConversationSummary[]> {
    const summaries = await this.client.chatbotsAll();
    const perBot = await Promise.all(
      summaries.map(async (summary) => {
        const botId = summary.id ?? "";
        const history = await this.client.conversationHistory(botId, userId);
        return (history.entries ?? []).map(
          (row): ConversationSummary => ({
            id: row.id ?? "",
            botKey: row.botKey ?? botId,
            title: row.title ?? "",
            dateLabel: formatDate(row.dateLabel),
            exchanges: (row.exchanges ?? []).map(toChatExample)
          })
        );
      })
    );
    return perBot.flat();
  }

  async Ask(conversationId: string, botKey: string, userId: string, title: string, question: string): Promise<string> {
    const response = await this.client.ask(botKey, conversationId, { userId, title, question });
    return response.answer ?? "";
  }

  async Delete(botKey: string, conversationId: string, userId: string): Promise<void> {
    await this.client.conversations(botKey, conversationId, userId);
  }
}
