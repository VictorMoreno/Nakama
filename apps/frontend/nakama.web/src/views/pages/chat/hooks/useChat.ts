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

type ThreadItem =
  | { kind: "user"; id: string; text: string }
  | { kind: "bot"; id: string; bot: BotMessageState }
  | { kind: "pending"; id: string };

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

export function useChat() {
  const { chatbots, currentBotKey } = useChatbots();
  const { isAdmin, currentUser } = useUser();
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
    if (!currentUser) return;
    let cancelled = false;
    conversationsRepo.FindAll(currentUser.id).then((data) => {
      if (!cancelled) setConversations(data);
    });
    return () => {
      cancelled = true;
    };
  }, [conversationsRepo, currentUser]);

  const botConversations = conversations.filter((conv) => conv.botKey === currentBotKey);

  function resetThread() {
    setActiveConversationId(null);
    setThread([]);
  }

  useEffect(() => {
    if (!bot) return;
    resetThread();
  }, [bot?.id]);

  useEffect(() => {
    threadRef.current?.scrollTo({ top: threadRef.current.scrollHeight });
  }, [thread]);

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

  function startNewConversation() {
    if (!bot) return;
    resetThread();
  }

  function handleReveal(botMsgId: string) {
    setThread((prev) =>
      prev.map((item) => (item.kind === "bot" && item.bot.id === botMsgId ? { ...item, bot: { ...item.bot, revealed: true } } : item))
    );
  }

  async function handleSend() {
    const val = input.trim();
    if (!val || !bot || !currentUser) return;

    const pendingId = nextId();
    setThread((prev) => [...prev, { kind: "user", id: nextId(), text: val }, { kind: "pending", id: pendingId }]);
    setInput("");
    setAttachedFiles([]);

    const isNewConversation = !activeConversationId;
    const conversationId = activeConversationId ?? crypto.randomUUID();
    if (isNewConversation) setActiveConversationId(conversationId);
    const title = val.length > 60 ? `${val.slice(0, 60)}…` : val;

    try {
      const answer = await conversationsRepo.Ask(conversationId, bot.id, currentUser.id, title, val);
      const example: ChatExample = { q: val, a: answer, cite: "", more: "", socratic: "" };

      if (isNewConversation) setActiveConversationId(conversationId);
      setThread((prev) => [
        ...prev.filter((item) => item.id !== pendingId),
        { kind: "bot", id: nextId(), bot: { id: nextId(), example, useSocratic: bot.settings.socratic, revealed: false } }
      ]);

      // Optimistic local update: `conversations` is only ever fetched once on mount, so without
      // this the new/updated conversation wouldn't show up in the sidebar until the component
      // remounts (e.g. leaving the page and coming back) and refetches for real.
      setConversations((prev) =>
        isNewConversation
          ? [{ id: conversationId, botKey: bot.id, title, dateLabel: new Date().toLocaleDateString("es-ES"), exchanges: [example] }, ...prev]
          : prev.map((c) => (c.id === conversationId ? { ...c, exchanges: [...c.exchanges, example] } : c))
      );
    } catch (error) {
      setThread((prev) => prev.filter((item) => item.id !== pendingId));
      const detail = error && typeof error === "object" && "detail" in error ? String((error as { detail?: unknown }).detail) : null;
      showToast(detail ?? "No se pudo obtener una respuesta del chatbot.");
    }
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
  // ConversationExchange still has no id of its own (a plain item in an embedded list, not a row
  // with identity), so per-message rating isn't viable - this stays the same "global useful-answer
  // rate" the Analytics dashboard already shows, not a per-message rating.
  function submitFeedback(useful: boolean) {
    void analyticsRepo.SubmitFeedback(useful);
  }

  async function deleteConversation(id: string) {
    if (!bot || !currentUser) return;

    await conversationsRepo.Delete(bot.id, id, currentUser.id);
    setConversations((prev) => prev.filter((c) => c.id !== id));

    if (activeConversationId === id) {
      resetThread();
    }
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
    startNewConversation,
    handleSend,
    handleReveal,
    submitFeedback,
    deleteConversation
  };
}
