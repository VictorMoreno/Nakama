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
 * only exists nested inside each chatbot's chat-page projection (chatPage(id).history), so
 * this fans out over chatbotsAll()+chatPage(id) per bot and flattens the result.
 */
export class ApiConversationsRepository implements IConversationsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(): Promise<ConversationSummary[]> {
    const summaries = await this.client.chatbotsAll();
    const perBot = await Promise.all(
      summaries.map(async (summary) => {
        const chatPage = await this.client.chatPage(summary.id);
        return chatPage.history.map(
          (row): ConversationSummary => ({
            id: row.id ?? "",
            botKey: row.botKey ?? summary.id,
            title: row.title ?? "",
            dateLabel: formatDate(row.dateLabel),
            exchanges: (row.exchanges ?? []).map(toChatExample)
          })
        );
      })
    );
    return perBot.flat();
  }
}
