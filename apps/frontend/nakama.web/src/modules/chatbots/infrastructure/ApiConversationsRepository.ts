import { authFetch } from "../../../shared/infrastructure/auth/authFetch";
import { formatDateTime } from "../../../shared/infrastructure/formatDate";
import type { GeneratedNakamaClient } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { ChatExample, ConversationSummary } from "../domain/Chatbot";
import type { IConversationsRepository } from "../domain/IConversationsRepository";

function toChatExample(row: {
  question?: string;
  answer?: string;
  attachments?: string[];
  useful?: boolean | null;
}): ChatExample {
  return {
    q: row.question ?? "",
    a: row.answer ?? "",
    attachments: row.attachments ?? [],
    useful: row.useful ?? null
  };
}

interface ConversationRow {
  startedAt: number;
  summary: ConversationSummary;
}

export class ApiConversationsRepository implements IConversationsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(): Promise<ConversationSummary[]> {
    const rows = await this.fetchRows();
    return rows.map((row) => row.summary);
  }

  async FindLatest(): Promise<ConversationSummary | null> {
    const rows = await this.fetchRows();
    const latest = rows.reduce<ConversationRow | null>((best, row) => (best === null || row.startedAt > best.startedAt ? row : best), null);
    return latest?.summary ?? null;
  }

  private async fetchRows(): Promise<ConversationRow[]> {
    const summaries = await this.client.chatbotsAll();
    const perBot = await Promise.all(
      summaries.map(async (summary) => {
        const botId = summary.id ?? "";
        const history = await this.client.conversationHistory(botId);
        return (history.entries ?? []).map(
          (row): ConversationRow => ({
            startedAt: row.dateLabel ? new Date(row.dateLabel).getTime() : 0,
            summary: {
              id: row.id ?? "",
              botKey: row.botKey ?? botId,
              title: row.title ?? "",
              dateLabel: formatDateTime(row.dateLabel),
              exchanges: (row.exchanges ?? []).map(toChatExample)
            }
          })
        );
      })
    );
    return perBot.flat();
  }

  async Ask(
    conversationId: string,
    botKey: string,
    title: string,
    question: string,
    files: File[],
    onChunk: (textSoFar: string) => void
  ): Promise<string> {
    const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";
    const form = new FormData();
    form.append("title", title);
    form.append("question", question);
    files.forEach((file) => form.append("files", file, file.name));

    const response = await authFetch(`${baseUrl}/api/chatbots/${botKey}/conversations/${conversationId}/ask`, {
      method: "POST",
      body: form
    });

    if (!response.ok || !response.body) {
      const problem = await response.json().catch(() => null);
      throw problem ?? new Error("No se pudo obtener una respuesta del chatbot.");
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    let buffer = "";
    let answer = "";
    let sawDone = false;

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      buffer += decoder.decode(value, { stream: true });

      let separatorIndex;
      while ((separatorIndex = buffer.indexOf("\n\n")) !== -1) {
        const frame = buffer.slice(0, separatorIndex);
        buffer = buffer.slice(separatorIndex + 2);
        const lines = frame.split("\n");
        const eventName = lines.find((l) => l.startsWith("event:"))?.slice(6).trim() ?? "chunk";
        const data = lines.find((l) => l.startsWith("data:"))?.slice(5).trim() ?? "";

        if (eventName === "chunk") {
          answer += JSON.parse(data) as string;
          onChunk(answer);
        } else if (eventName === "done") {
          sawDone = true;
        } else if (eventName === "error") {
          throw JSON.parse(data);
        }
      }
    }

    if (!sawDone) {
      throw new Error("La respuesta se interrumpió antes de completarse.");
    }

    return answer;
  }

  async RateExchange(botKey: string, conversationId: string, exchangeIndex: number, useful: boolean): Promise<void> {
    await this.client.feedback(botKey, conversationId, exchangeIndex, { useful });
  }

  async Delete(conversationId: string): Promise<void> {
    await this.client.conversations(conversationId);
  }
}
