import { useEffect, useRef, useState } from "react";
import { useChatbots } from "../../shared/contexts/ChatbotsContext";
import { ChevronDownIcon } from "../shared/icons";
import styles from "./BotSwitcher.module.scss";

export default function BotSwitcher() {
  const { activeChatbots, currentBotKey, setCurrentBotKey } = useChatbots();
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const rootRef = useRef<HTMLDivElement>(null);
  const searchRef = useRef<HTMLInputElement>(null);

  const current = activeChatbots.find((b) => b.key === currentBotKey);
  const matches = activeChatbots.filter((b) => b.label.toLowerCase().includes(query.trim().toLowerCase()));

  useEffect(() => {
    if (!open) return;
    function onPointerDown(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false);
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  function handleOpen() {
    setQuery("");
    setOpen(true);
    setTimeout(() => searchRef.current?.focus(), 0);
  }

  return (
    <div className={`${styles.botSwitcher}${open ? ` ${styles.open}` : ""}`} ref={rootRef}>
      <button
        className={styles.botSwitcherTrigger}
        type="button"
        aria-haspopup="true"
        aria-expanded={open}
        onClick={() => (open ? setOpen(false) : handleOpen())}
      >
        <span className={styles.botSwitcherDot} />
        <span className={styles.botSwitcherLabel}>{current?.label ?? "Selecciona un chatbot"}</span>
        <ChevronDownIcon className={styles.chevron} />
      </button>

      <div className={styles.botSwitcherDropdown}>
        <input
          ref={searchRef}
          type="text"
          className={styles.botSwitcherSearch}
          placeholder="Buscar chatbot…"
          aria-label="Buscar chatbot"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onClick={(e) => e.stopPropagation()}
        />
        <div className={styles.botSwitcherList}>
          {matches.length === 0 && <div className={styles.botSwitcherEmpty}>Ningún chatbot coincide con tu búsqueda</div>}
          {matches.map((bot) => (
            <button
              key={bot.key}
              className={styles.botSwitcherRow}
              type="button"
              aria-current={bot.key === currentBotKey}
              onClick={() => {
                setCurrentBotKey(bot.key);
                setOpen(false);
              }}
            >
              <span>{bot.label}</span>
              <span className={styles.meta}>{bot.lastUsed}</span>
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
