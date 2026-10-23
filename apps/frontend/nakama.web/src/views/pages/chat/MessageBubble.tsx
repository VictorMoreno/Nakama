import { useState } from "react";
import { AttachIcon, BrandMarkIcon, ThumbDownIcon, ThumbUpIcon } from "../../components/shared/icons";
import Markdown from "../../components/shared/Markdown";
import { useToast } from "../../shared/contexts/ToastContext";
import type { ChatExample, ChatbotSettings } from "../../../modules/chatbots/domain/Chatbot";
import styles from "./MessageBubble.module.scss";

export interface BotMessageState {
  id: string;
  example: ChatExample;
  useSocratic: boolean;
  revealed: boolean;
  conversationId: string;
  exchangeIndex: number;
  complete: boolean;
}

interface UserBubbleProps {
  text: string;
  attachments?: string[];
}

export function UserBubble({ text, attachments = [] }: UserBubbleProps) {
  return (
    <div className={`${styles.msg} ${styles.user}`}>
      <div className={styles.msgAvatar}>TÚ</div>
      <div className={styles.bubble}>
        <span className={styles.plainText}>{text}</span>
        {attachments.length > 0 && (
          <div className={styles.bubbleAttachments}>
            {attachments.map((name, i) => (
              <span className={styles.bubbleAttachment} key={`${name}-${i}`} title={name}>
                <AttachIcon />
                <span className={styles.bubbleAttachmentName}>{name}</span>
              </span>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

// Shown instead of BotBubble when there's no real exchange yet (new/empty conversation) -
// just a friendly greeting from the bot
// itself, none of BotBubble's Q&A chrome (no citation chip, no feedback, no "explícame más"),
// since there's no actual answer here to cite or rate.
export function WelcomeBubble() {
  return (
    <div className={`${styles.msg} ${styles.bot}`}>
      <div className={styles.msgAvatar}>
        <BrandMarkIcon />
      </div>
      <div className={styles.bubble}>
        <span>
          ¡Hola! Estoy a tu disposición para resolver cualquier duda que tengas sobre este chatbot. Preguntame lo que necesites, ¡vamos a
          resolverlo juntos! 💪
        </span>
      </div>
    </div>
  );
}

export function PendingBubble() {
  return (
    <div className={`${styles.msg} ${styles.bot}`}>
      <div className={styles.msgAvatar}>
        <BrandMarkIcon />
      </div>
      <div className={styles.bubble}>
        <span>Escribiendo…</span>
      </div>
    </div>
  );
}

interface BotBubbleProps {
  state: BotMessageState;
  settings: ChatbotSettings;
  onReveal: (id: string) => void;
  onFeedback: (id: string, useful: boolean) => void;
}

export function BotBubble({ state, settings, onReveal, onFeedback }: BotBubbleProps) {
  const [moreOpen, setMoreOpen] = useState(false);
  const { showToast } = useToast();
  const rated = state.example.useful ?? null;

  function handleFeedback(useful: boolean) {
    if (rated === useful) return;
    onFeedback(state.id, useful);
    showToast(useful ? "Gracias por tu valoración" : "Gracias, lo tendremos en cuenta");
  }

  const showSocratic = state.useSocratic && !state.revealed && state.example.socratic !== "";

  return (
    <div className={`${styles.msg} ${styles.bot}`}>
      <div className={styles.msgAvatar}>
        <BrandMarkIcon />
      </div>
      <div className={styles.bubble}>
        {showSocratic ? (
          <div>
            <Markdown>{state.example.socratic}</Markdown>
            <div className={styles.msgActions}>
              <button className={styles.msgAction} type="button" onClick={() => onReveal(state.id)}>
                Ver respuesta directa
              </button>
            </div>
          </div>
        ) : (
          <div>
            <Markdown>{state.example.a}</Markdown>
            {state.example.cite !== "" && (
              <button className={styles.citationChip} type="button">
                ◆ {state.example.cite}
              </button>
            )}

            {settings.explicaMas && state.example.more !== "" && (
              <div className={styles.msgActions}>
                <button className={styles.msgAction} type="button" onClick={() => setMoreOpen((o) => !o)}>
                  {moreOpen ? "Ocultar detalle" : "Explícame más"}
                </button>
              </div>
            )}

            {settings.explicaMas && state.example.more !== "" && (
              <div className={`${styles.layer}${moreOpen ? ` ${styles.open}` : ""}`}>
                <Markdown>{state.example.more}</Markdown>
              </div>
            )}

            <div className={styles.msgFeedback}>
              <span className={styles.fbLabel}>¿Te ha sido útil?</span>
              <button
                className={styles.feedbackBtn}
                data-fb="up"
                aria-pressed={rated === true}
                aria-label="Respuesta útil"
                type="button"
                disabled={!state.complete}
                onClick={() => handleFeedback(true)}
              >
                <ThumbUpIcon />
              </button>
              <button
                className={styles.feedbackBtn}
                data-fb="down"
                aria-pressed={rated === false}
                aria-label="Respuesta no útil"
                type="button"
                disabled={!state.complete}
                onClick={() => handleFeedback(false)}
              >
                <ThumbDownIcon />
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
