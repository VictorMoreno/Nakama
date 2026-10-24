import { useEffect, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import type { ChatExample, ConversationSummary } from "../../../../modules/chatbots/domain/Chatbot";
import type { IConversationsRepository } from "../../../../modules/chatbots/domain/IConversationsRepository";
import { chatConfig } from "../../../../shared/infrastructure/config/ChatConfig";
import { formatDateTime } from "../../../../shared/infrastructure/formatDate";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUser } from "../../../shared/contexts/UserContext";
import type { BotMessageState } from "../MessageBubble";

type ThreadItem =
  | { kind: "user"; id: string; text: string; attachments: string[] }
  | { kind: "bot"; id: string; bot: BotMessageState }
  | { kind: "pending"; id: string };

let uidCounter = 0;
function nextId() {
  uidCounter += 1;
  return `m${uidCounter}`;
}

function exchangesToThread(exchanges: ChatExample[], conversationId: string): ThreadItem[] {
  return exchanges.flatMap((example, exchangeIndex) => [
    { kind: "user" as const, id: nextId(), text: example.q, attachments: example.attachments ?? [] },
    {
      kind: "bot" as const,
      id: nextId(),
      bot: { id: nextId(), example, conversationId, exchangeIndex, complete: true }
    }
  ]);
}

// Caps how tall the composer textarea auto-grows before it scrolls internally instead
// (about 6 lines at the composer's font size) — otherwise a long pasted question could
// push the send row off-screen.
const MAX_COMPOSER_HEIGHT = 160;

const MAX_TITLE_LENGTH = 60;

export function useChat() {
  const { chatbots, currentBotKey, setCurrentBotKey } = useChatbots();
  const { isAdmin, currentUser } = useUser();
  const conversationsRepo = useInjection<IConversationsRepository>(Services.ConversationsRepository.id);
  const navigate = useNavigate();
  const location = useLocation();
  const { showToast } = useToast();
  const bot = chatbots[currentBotKey];

  const [conversations, setConversations] = useState<ConversationSummary[]>([]);
  const [thread, setThread] = useState<ThreadItem[]>([]);
  const [input, setInput] = useState("");
  const [attachedFiles, setAttachedFiles] = useState<File[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
  const threadRef = useRef<HTMLDivElement>(null);
  const composerRef = useRef<HTMLTextAreaElement>(null);
  const pendingOpenRef = useRef<ConversationSummary | null>(null);
  const requestedConversationId = (location.state as { conversationId?: string } | null)?.conversationId;
  const deepLinkRef = useRef({ requestedConversationId, currentBotKey, pathname: location.pathname, navigate, setCurrentBotKey });

  useEffect(() => {
    deepLinkRef.current = { requestedConversationId, currentBotKey, pathname: location.pathname, navigate, setCurrentBotKey };
  });

  useEffect(() => {
    if (!currentUser) return;
    let cancelled = false;
    conversationsRepo.FindAll().then((data) => {
      if (cancelled) return;
      setConversations(data);
      const link = deepLinkRef.current;
      const requested = data.find((conv) => conv.id === link.requestedConversationId);
      if (!requested) return;
      link.navigate(link.pathname, { replace: true, state: null });
      if (requested.botKey === link.currentBotKey) {
        openConversation(requested);
      } else {
        pendingOpenRef.current = requested;
        link.setCurrentBotKey(requested.botKey);
      }
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
    const pending = pendingOpenRef.current;
    if (pending && pending.botKey === bot.id) {
      pendingOpenRef.current = null;
      openConversation(pending);
    }
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
    setThread(exchangesToThread(conv.exchanges, conv.id));
    setActiveConversationId(conv.id);
  }

  function startNewConversation() {
    if (!bot) return;
    resetThread();
  }

  async function sendMessage(text: string, files: File[]) {
    const val = text.trim();
    if (!val || !bot || !currentUser) return;

    const pendingId = nextId();
    const attachmentNames = files.map((file) => file.name);
    setThread((prev) => [
      ...prev,
      { kind: "user", id: nextId(), text: val, attachments: attachmentNames },
      { kind: "pending", id: pendingId }
    ]);

    const isNewConversation = !activeConversationId;
    const conversationId = activeConversationId ?? crypto.randomUUID();
    if (isNewConversation) setActiveConversationId(conversationId);
    const title = val.length > MAX_TITLE_LENGTH ? `${val.slice(0, MAX_TITLE_LENGTH)}…` : val;

    const botMsgId = nextId();
    const exchangeIndex = conversations.find((c) => c.id === conversationId)?.exchanges.length ?? 0;

    try {
      const answer = await conversationsRepo.Ask(conversationId, bot.id, title, val, files, (textSoFar) => {
        setThread((prev) => {
          const alreadyStarted = prev.some((item) => item.id === botMsgId);
          const example: ChatExample = { q: val, a: textSoFar, attachments: attachmentNames };
          if (!alreadyStarted) {
            return [
              ...prev.filter((item) => item.id !== pendingId),
              {
                kind: "bot",
                id: botMsgId,
                bot: {
                  id: botMsgId,
                  example,
                  conversationId,
                  exchangeIndex,
                  complete: false
                }
              }
            ];
          }
          return prev.map((item) =>
            item.kind === "bot" && item.id === botMsgId
              ? { ...item, bot: { ...item.bot, example: { ...item.bot.example, a: textSoFar } } }
              : item
          );
        });
      });
      const example: ChatExample = { q: val, a: answer, attachments: attachmentNames };

      if (isNewConversation) setActiveConversationId(conversationId);

      setThread((prev) =>
        prev.map((item) => (item.kind === "bot" && item.id === botMsgId ? { ...item, bot: { ...item.bot, complete: true } } : item))
      );
      setConversations((prev) =>
        isNewConversation
          ? [{ id: conversationId, botKey: bot.id, title, dateLabel: formatDateTime(new Date()), exchanges: [example] }, ...prev]
          : prev.map((c) => (c.id === conversationId ? { ...c, exchanges: [...c.exchanges, example] } : c))
      );
    } catch (error) {
      setThread((prev) => prev.filter((item) => item.id !== pendingId && item.id !== botMsgId));
      const detail = error && typeof error === "object" && "detail" in error ? String((error as { detail?: unknown }).detail) : null;
      showToast(detail ?? "No se pudo obtener una respuesta del chatbot.");
    }
  }

  async function handleSend() {
    const files = attachedFiles;
    const text = input;
    if (!text.trim() || !bot || !currentUser) return;

    setInput("");
    setAttachedFiles([]);
    await sendMessage(text, files);
  }

  async function sendFollowUp(text: string) {
    await sendMessage(text, []);
  }

  function handleAttachFiles(files: FileList | null) {
    if (!files || files.length === 0) return;

    const incoming = Array.from(files);
    const maxAttachmentBytes = chatConfig.maxAttachmentSizeMb * 1024 * 1024;
    const valid = incoming.filter((file) => {
      const lowerName = file.name.toLowerCase();
      return chatConfig.allowedAttachmentExtensions.some((ext) => lowerName.endsWith(ext)) && file.size <= maxAttachmentBytes;
    });
    const room = chatConfig.maxAttachments - attachedFiles.length;
    const accepted = valid.slice(0, Math.max(room, 0));

    if (accepted.length > 0) {
      setAttachedFiles((prev) => [...prev, ...accepted]);
    }

    if (accepted.length < incoming.length) {
      showToast(
        `Solo se admiten hasta ${chatConfig.maxAttachments} archivos (${chatConfig.allowedAttachmentExtensions.join(", ")}) de hasta ${chatConfig.maxAttachmentSizeMb} MB.`
      );
    }
  }

  function removeAttachedFile(index: number) {
    setAttachedFiles((prev) => prev.filter((_, i) => i !== index));
  }

  function applyRating(botMsgId: string, conversationId: string, exchangeIndex: number, useful: boolean | null) {
    setThread((prev) =>
      prev.map((item) =>
        item.kind === "bot" && item.bot.id === botMsgId ? { ...item, bot: { ...item.bot, example: { ...item.bot.example, useful } } } : item
      )
    );
    setConversations((prev) =>
      prev.map((c) =>
        c.id === conversationId ? { ...c, exchanges: c.exchanges.map((e, i) => (i === exchangeIndex ? { ...e, useful } : e)) } : c
      )
    );
  }

  function submitFeedback(botMsgId: string, useful: boolean) {
    const item = thread.find((candidate) => candidate.kind === "bot" && candidate.bot.id === botMsgId);
    if (!bot || !item || item.kind !== "bot") return;

    const { conversationId, exchangeIndex } = item.bot;
    const previous = item.bot.example.useful ?? null;
    applyRating(botMsgId, conversationId, exchangeIndex, useful);
    conversationsRepo.RateExchange(bot.id, conversationId, exchangeIndex, useful).catch(() => {
      applyRating(botMsgId, conversationId, exchangeIndex, previous);
      showToast("No se pudo guardar tu valoración.");
    });
  }

  async function deleteConversation(id: string) {
    if (!bot || !currentUser) return;

    await conversationsRepo.Delete(id);
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
    sendFollowUp,
    submitFeedback,
    deleteConversation
  };
}
