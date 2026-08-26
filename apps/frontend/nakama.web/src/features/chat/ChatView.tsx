import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useChatbots } from "../../context/ChatbotsContext";
import { mockConversations } from "../../data/mockConversations";
import type { ChatExample, ConversationSummary } from "../../types/chatbot";
import { BotBubble, UserBubble, type BotMessageState } from "./MessageBubble";

type ThreadItem = { kind: "user"; id: string; text: string } | { kind: "bot"; id: string; bot: BotMessageState };

let uidCounter = 0;
function nextId() {
  uidCounter += 1;
  return `m${uidCounter}`;
}

function exchangesToThread(exchanges: ChatExample[], useSocratic: boolean): ThreadItem[] {
  return exchanges.flatMap((example) => [
    { kind: "user" as const, id: nextId(), text: example.q },
    { kind: "bot" as const, id: nextId(), bot: { id: nextId(), example, useSocratic, revealed: false } }
  ]);
}

const DEMO_FALLBACK: ChatExample = {
  q: "",
  a: "Este es un prototipo de demostración — solo la pregunta de ejemplo que ves al abrir cada chatbot tiene una respuesta guionizada, para mostrar con precisión cómo se comportan las funcionalidades diferenciadoras (citas, profundidad y modo socrático).",
  cite: "Modo demo",
  more: "En la versión real, esta respuesta vendría del contenido indexado del chatbot seleccionado, para cualquier pregunta.",
  socratic: "¿Tu pregunta se parece a la conversación de ejemplo que ya ves arriba?"
};

export function ChatView() {
  const { chatbots, currentBotKey, isAdmin } = useChatbots();
  const navigate = useNavigate();
  const bot = chatbots[currentBotKey];
  const botConversations = mockConversations.filter((conv) => conv.botKey === currentBotKey);

  const [thread, setThread] = useState<ThreadItem[]>([]);
  const [input, setInput] = useState("");
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
  const threadRef = useRef<HTMLDivElement>(null);

  // Intentionally keyed only on bot.key: this should reset the thread when switching
  // chatbots, not when the current bot's settings change mid-conversation.
  useEffect(() => {
    if (!bot) return;
    setActiveConversationId(null);
    if (bot.opening) {
      setThread(exchangesToThread([bot.opening], bot.settings.socratic));
    } else {
      setThread([]);
    }
  }, [bot?.key]);

  useEffect(() => {
    threadRef.current?.scrollTo({ top: threadRef.current.scrollHeight });
  }, [thread]);

  function openConversation(conv: ConversationSummary) {
    setThread(exchangesToThread(conv.exchanges, bot!.settings.socratic));
    setActiveConversationId(conv.id);
  }

  function appendExchange(question: string, example: ChatExample) {
    setActiveConversationId(null);
    setThread((prev) => [
      ...prev,
      { kind: "user", id: nextId(), text: question },
      { kind: "bot", id: nextId(), bot: { id: nextId(), example, useSocratic: bot!.settings.socratic, revealed: false } }
    ]);
  }

  function handleReveal(botMsgId: string) {
    setThread((prev) =>
      prev.map((item) => (item.kind === "bot" && item.bot.id === botMsgId ? { ...item, bot: { ...item.bot, revealed: true } } : item))
    );
  }

  function handleSend() {
    const val = input.trim();
    if (!val || !bot) return;

    const match = bot.chips.find((c) => c.q === val) ?? (bot.opening && bot.opening.q === val ? bot.opening : null);
    appendExchange(val, match ?? DEMO_FALLBACK);
    setInput("");
  }

  if (!bot) {
    return (
      <section className="view active">
        <div className="glass" style={{ padding: 24 }}>
          <p>
            No tienes ningún chatbot seleccionado.{" "}
            {isAdmin ? (
              <button className="config-link" type="button" onClick={() => navigate("/chatbots")}>
                Ir a Gestión de chatbots →
              </button>
            ) : (
              "Elige uno con el selector de la parte superior."
            )}
          </p>
        </div>
      </section>
    );
  }

  return (
    <section className="view active">
      <div className="chat-shell">
        <div className="chat-panel glass">
          <div className="chat-panel-head">
            <div className="bot-id">
              <div className="bot-avatar" />
              <div className="bot-id-text">
                <div className="name">Bot · {bot.label}</div>
                <div className="status">En línea</div>
              </div>
            </div>
          </div>

          <div className="thread" ref={threadRef}>
            {thread.length === 0 && (
              <div className="thread-empty">
                Este chatbot todavía no tiene documentos indexados. En cuanto se suba contenido en <strong>Documentos</strong>, podrás
                empezar a preguntar aquí.
              </div>
            )}
            {thread.map((item) =>
              item.kind === "user" ? (
                <UserBubble key={item.id} text={item.text} />
              ) : (
                <BotBubble key={item.id} state={item.bot} settings={bot.settings} onReveal={handleReveal} />
              )
            )}
          </div>

          <div className="composer">
            <input
              type="text"
              placeholder="Escribe tu pregunta…"
              aria-label="Escribe tu pregunta"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") handleSend();
              }}
            />
            <button type="button" onClick={handleSend}>
              Enviar
            </button>
          </div>
        </div>

        <div className="side-col">
          <div className="context-card glass">
            <span className="eyebrow">Sobre este bot</span>
            <p dangerouslySetInnerHTML={{ __html: bot.context }} />
            {isAdmin && (
              <button className="config-link" type="button" onClick={() => navigate(`/chatbots/${bot.key}?tab=config`)}>
                Ver configuración del chatbot →
              </button>
            )}
          </div>

          <div className="history-card glass">
            <span className="eyebrow">Historial de conversaciones</span>
            <div className="history-panel">
              {botConversations.length === 0 ? (
                <div className="history-empty">Todavía no tienes conversaciones guardadas con este chatbot.</div>
              ) : (
                botConversations.map((conv) => (
                  <button
                    key={conv.id}
                    className={`history-row${conv.id === activeConversationId ? " active" : ""}`}
                    type="button"
                    onClick={() => openConversation(conv)}
                  >
                    <span className="history-row-title">{conv.title}</span>
                    <span className="history-row-date">{conv.dateLabel}</span>
                  </button>
                ))
              )}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

