import type { GeneratedNakamaClient } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { ChatExample, ConversationSummary } from "../domain/Chatbot";
import type { IConversationsRepository } from "../domain/IConversationsRepository";

function formatDate(value: Date | string | undefined): string {
  if (!value) return "";
  const date = typeof value === "string" ? new Date(value) : value;
  return Number.isNaN(date.getTime()) ? "" : date.toLocaleDateString("es-ES");
}

function toChatExample(row: {
  question?: string;
  answer?: string;
  citation?: string;
  moreExplanation?: string;
  socraticVariant?: string;
  attachments?: string[];
}): ChatExample {
  return {
    q: row.question ?? "",
    a: row.answer ?? "",
    cite: row.citation ?? "",
    more: row.moreExplanation ?? "",
    socratic: row.socraticVariant ?? "",
    attachments: row.attachments ?? []
  };
}

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

  async Ask(
    conversationId: string,
    botKey: string,
    userId: string,
    title: string,
    question: string,
    files: File[],
    onChunk: (textSoFar: string) => void
  ): Promise<string> {
    const baseUrl = import.meta.env.VITE_API_BASE_URL ?? "";
    const form = new FormData();
    form.append("userId", userId);
    form.append("title", title);
    form.append("question", question);
    files.forEach((file) => form.append("files", file, file.name));

    const response = await fetch(`${baseUrl}/api/chatbots/${botKey}/conversations/${conversationId}/ask`, {
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

  async Delete(conversationId: string, userId: string): Promise<void> {
    await this.client.conversations(conversationId, userId);
  }
}
