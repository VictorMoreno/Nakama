import { useChat } from "./hooks/useChat";
import { BotBubble, UserBubble } from "./MessageBubble";
import styles from "./ChatPage.module.scss";

export default function ChatPage() {
  const {
    bot,
    isAdmin,
    navigate,
    thread,
    threadRef,
    input,
    setInput,
    botConversations,
    activeConversationId,
    openConversation,
    handleSend,
    handleReveal
  } = useChat();

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
      <div className={styles.chatShell}>
        <div className={`${styles.chatPanel} glass`}>
          <div className={styles.chatPanelHead}>
            <div className={styles.botId}>
              <div className={styles.botAvatar} />
              <div className={styles.botIdText}>
                <div className={styles.name}>Bot · {bot.label}</div>
                <div className={styles.status}>En línea</div>
              </div>
            </div>
          </div>

          <div className={styles.thread} ref={threadRef}>
            {thread.length === 0 && (
              <div className={styles.threadEmpty}>
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

          <div className={styles.composer}>
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

        <div className={styles.sideCol}>
          <div className={`${styles.contextCard} glass`}>
            <span className="eyebrow">Sobre este bot</span>
            <p dangerouslySetInnerHTML={{ __html: bot.context }} />
            {isAdmin && (
              <button className="config-link" type="button" onClick={() => navigate(`/chatbots/${bot.key}?tab=config`)}>
                Ver configuración del chatbot →
              </button>
            )}
          </div>

          <div className={`${styles.historyCard} glass`}>
            <span className="eyebrow">Historial de conversaciones</span>
            <div className={styles.historyPanel}>
              {botConversations.length === 0 ? (
                <div className={styles.historyEmpty}>Todavía no tienes conversaciones guardadas con este chatbot.</div>
              ) : (
                botConversations.map((conv) => (
                  <button
                    key={conv.id}
                    className={`${styles.historyRow}${conv.id === activeConversationId ? ` ${styles.active}` : ""}`}
                    type="button"
                    onClick={() => openConversation(conv)}
                  >
                    <span className={styles.historyRowTitle}>{conv.title}</span>
                    <span className={styles.historyRowDate}>{conv.dateLabel}</span>
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
