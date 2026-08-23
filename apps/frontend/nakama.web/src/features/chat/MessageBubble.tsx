import { useState } from "react";
import type { ChatExample, ChatbotSettings } from "../../types/chatbot";
import { ThumbDownIcon, ThumbUpIcon } from "../../components/icons";
import { useToast } from "../../context/ToastContext";

export interface BotMessageState {
  id: string;
  example: ChatExample;
  useSocratic: boolean;
  revealed: boolean;
}

interface UserBubbleProps {
  text: string;
}

export function UserBubble({ text }: UserBubbleProps) {
  return (
    <div className="msg user">
      <div className="msg-avatar">TÚ</div>
      <div className="bubble" dangerouslySetInnerHTML={{ __html: text }} />
    </div>
  );
}

interface BotBubbleProps {
  state: BotMessageState;
  settings: ChatbotSettings;
  onReveal: (id: string) => void;
}

type Feedback = "up" | "down" | null;

export function BotBubble({ state, settings, onReveal }: BotBubbleProps) {
  const [moreOpen, setMoreOpen] = useState(false);
  const [feedback, setFeedback] = useState<Feedback>(null);
  const { showToast } = useToast();

  function handleFeedback(choice: "up" | "down") {
    setFeedback((f) => {
      const next = f === choice ? null : choice;
      if (next) showToast(next === "up" ? "Gracias por tu valoración" : "Gracias, lo tendremos en cuenta");
      return next;
    });
  }

  const showSocratic = state.useSocratic && !state.revealed;

  return (
    <div className="msg bot">
      <div className="msg-avatar">🤖</div>
      <div className="bubble">
        {showSocratic ? (
          <div className="socratic-block">
            <span dangerouslySetInnerHTML={{ __html: state.example.socratic }} />
            <div className="msg-actions">
              <button className="msg-action" type="button" onClick={() => onReveal(state.id)}>
                Ver respuesta directa
              </button>
            </div>
          </div>
        ) : (
          <div className="direct-block">
            <span dangerouslySetInnerHTML={{ __html: state.example.a }} />
            <button className="citation-chip" type="button">
              ◆ {state.example.cite}
            </button>

            {settings.explicaMas && (
              <div className="msg-actions">
                <button className="msg-action" type="button" onClick={() => setMoreOpen((o) => !o)}>
                  {moreOpen ? "Ocultar detalle" : "Explícame más"}
                </button>
              </div>
            )}

            {settings.explicaMas && (
              <div className={`layer${moreOpen ? " open" : ""}`} dangerouslySetInnerHTML={{ __html: state.example.more }} />
            )}

            <div className="msg-feedback">
              <span className="fb-label">¿Te ha sido útil?</span>
              <button
                className="feedback-btn"
                data-fb="up"
                aria-pressed={feedback === "up"}
                aria-label="Respuesta útil"
                type="button"
                onClick={() => handleFeedback("up")}
              >
                <ThumbUpIcon />
              </button>
              <button
                className="feedback-btn"
                data-fb="down"
                aria-pressed={feedback === "down"}
                aria-label="Respuesta no útil"
                type="button"
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
