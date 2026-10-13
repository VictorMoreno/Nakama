import { useState } from "react";
import { AttachIcon, BrandMarkIcon, ThumbDownIcon, ThumbUpIcon } from "../../components/shared/icons";
import { useToast } from "../../shared/contexts/ToastContext";
import type { ChatExample, ChatbotSettings } from "../../../modules/chatbots/domain/Chatbot";
import styles from "./MessageBubble.module.scss";

export interface BotMessageState {
  id: string;
  example: ChatExample;
  useSocratic: boolean;
  revealed: boolean;
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
        <span dangerouslySetInnerHTML={{ __html: text }} />
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
  onFeedback: (useful: boolean) => void;
}

type Feedback = "up" | "down" | null;

export function BotBubble({ state, settings, onReveal, onFeedback }: BotBubbleProps) {
  const [moreOpen, setMoreOpen] = useState(false);
  const [feedback, setFeedback] = useState<Feedback>(null);
  const { showToast } = useToast();

  function handleFeedback(choice: "up" | "down") {
    if (feedback) return;
    setFeedback(choice);
    onFeedback(choice === "up");
    showToast(choice === "up" ? "Gracias por tu valoración" : "Gracias, lo tendremos en cuenta");
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
            <span dangerouslySetInnerHTML={{ __html: state.example.socratic }} />
            <div className={styles.msgActions}>
              <button className={styles.msgAction} type="button" onClick={() => onReveal(state.id)}>
                Ver respuesta directa
              </button>
            </div>
          </div>
        ) : (
          <div>
            <span dangerouslySetInnerHTML={{ __html: state.example.a }} />
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
              <div className={`${styles.layer}${moreOpen ? ` ${styles.open}` : ""}`} dangerouslySetInnerHTML={{ __html: state.example.more }} />
            )}

            <div className={styles.msgFeedback}>
              <span className={styles.fbLabel}>¿Te ha sido útil?</span>
              <button
                className={styles.feedbackBtn}
                data-fb="up"
                aria-pressed={feedback === "up"}
                aria-label="Respuesta útil"
                type="button"
                disabled={feedback !== null}
                onClick={() => handleFeedback("up")}
              >
                <ThumbUpIcon />
              </button>
              <button
                className={styles.feedbackBtn}
                data-fb="down"
                aria-pressed={feedback === "down"}
                aria-label="Respuesta no útil"
                type="button"
                disabled={feedback !== null}
                onClick={() => handleFeedback("down")}
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
