import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { IAnalyticsRepository } from "../../../../modules/analytics/domain/IAnalyticsRepository";
import type { ChatExample, ConversationSummary } from "../../../../modules/chatbots/domain/Chatbot";
import type { IConversationsRepository } from "../../../../modules/chatbots/domain/IConversationsRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUser } from "../../../shared/contexts/UserContext";
import type { BotMessageState } from "../MessageBubble";

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

// Caps how tall the composer textarea auto-grows before it scrolls internally instead
// (about 6 lines at the composer's font size) — otherwise a long pasted question could
// push the send row off-screen.
const MAX_COMPOSER_HEIGHT = 160;

const DEMO_FALLBACK: ChatExample = {
  q: "",
  a: "Este es un prototipo de demostración — solo la pregunta de ejemplo que ves al abrir cada chatbot tiene una respuesta guionizada, para mostrar con precisión cómo se comportan las funcionalidades diferenciadoras (citas, profundidad y modo socrático).",
  cite: "Modo demo",
  more: "En la versión real, esta respuesta vendría del contenido indexado del chatbot seleccionado, para cualquier pregunta.",
  socratic: "¿Tu pregunta se parece a la conversación de ejemplo que ya ves arriba?"
};

export function useChat() {
  const { chatbots, currentBotKey } = useChatbots();
  const { isAdmin } = useUser();
  const conversationsRepo = useInjection<IConversationsRepository>(Services.ConversationsRepository.id);
  const analyticsRepo = useInjection<IAnalyticsRepository>(Services.AnalyticsRepository.id);
  const navigate = useNavigate();
  const { showToast } = useToast();
  const bot = chatbots[currentBotKey];

  const [conversations, setConversations] = useState<ConversationSummary[]>([]);
  const [thread, setThread] = useState<ThreadItem[]>([]);
  const [input, setInput] = useState("");
  const [attachedFiles, setAttachedFiles] = useState<File[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
  const threadRef = useRef<HTMLDivElement>(null);
  const composerRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    let cancelled = false;
    conversationsRepo.FindAll().then((data) => {
      if (!cancelled) setConversations(data);
    });
    return () => {
      cancelled = true;
    };
  }, [conversationsRepo]);

  const botConversations = conversations.filter((conv) => conv.botKey === currentBotKey);

  // Intentionally keyed only on bot.key: this should reset the thread when switching
  // chatbots, not when the current bot's settings change mid-conversation. A `key`-prop
  // reset isn't viable here since ChatPage itself doesn't remount on bot switch (the rail
  // stays mounted), so the effect is the correct tool despite the lint rule's default bias
  // toward computing state during render.
  /* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
  useEffect(() => {
    if (!bot) return;
    setActiveConversationId(null);
    if (bot.opening) {
      setThread(exchangesToThread([bot.opening], bot.settings.socratic));
    } else {
      setThread([]);
    }
  }, [bot?.id]);
  /* eslint-enable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */

  useEffect(() => {
    threadRef.current?.scrollTo({ top: threadRef.current.scrollHeight });
  }, [thread]);

  // Auto-grows the composer with its content (up to MAX_COMPOSER_HEIGHT) instead of
  // staying pinned to a single line — resetting to "auto" first is what lets it shrink
  // back down again after deleting text, not just grow. overflowY only turns on once
  // content actually exceeds the cap, otherwise the browser reserves a scrollbar gutter
  // (or shows one outright) for a box that never needs to scroll.
  useEffect(() => {
    const el = composerRef.current;
    if (!el) return;
    el.style.height = "auto";
    const needsScroll = el.scrollHeight > MAX_COMPOSER_HEIGHT;
    el.style.height = `${Math.min(el.scrollHeight, MAX_COMPOSER_HEIGHT)}px`;
    el.style.overflowY = needsScroll ? "auto" : "hidden";
  }, [input]);

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
    setAttachedFiles([]);
  }

  // Attaching only stages files locally for this prototype — there's no upload
  // repository to send them to yet (no IChatbotsRepository method for it), so this is
  // a demo-only affordance, same spirit as the app's other "(demo)" actions.
  function handleAttachFiles(files: FileList | null) {
    if (!files || files.length === 0) return;
    setAttachedFiles((prev) => [...prev, ...Array.from(files)]);
    showToast(`${files.length} archivo${files.length === 1 ? "" : "s"} adjuntado${files.length === 1 ? "" : "s"} (demo)`);
  }

  function removeAttachedFile(index: number) {
    setAttachedFiles((prev) => prev.filter((_, i) => i !== index));
  }

  // Global feedback (👍/👎), decoupled from any specific exchange - see AnalyticsFeedbackSubmitter:
  // today's chat is demo-only and never persists a real, identifiable exchange server-side, so this
  // is the same "global useful-answer rate" the Analytics dashboard already shows, not a per-message rating.
  function submitFeedback(useful: boolean) {
    void analyticsRepo.SubmitFeedback(useful);
  }

  return {
    bot,
    isAdmin,
    navigate,
    thread,
    threadRef,
    composerRef,
    input,
    setInput,
    attachedFiles,
    handleAttachFiles,
    removeAttachedFile,
    botConversations,
    activeConversationId,
    openConversation,
    handleSend,
    handleReveal,
    submitFeedback
  };
}
