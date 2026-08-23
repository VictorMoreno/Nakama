import { useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { StatusSelect, type StatusSelectOption } from "../../components/StatusSelect";
import { useChatbots } from "../../context/ChatbotsContext";
import { CHATBOT_STATUS_META } from "../../lib/status";
import { botSubText } from "../../lib/text";
import type { ChatbotLifecycleStatus } from "../../types/chatbot";
import { AnalyticsTab } from "./tabs/AnalyticsTab";
import { DocumentsTab } from "./tabs/DocumentsTab";
import { GeneralTab } from "./tabs/GeneralTab";
import { PermissionsTab } from "./tabs/PermissionsTab";

type TabKey = "config" | "docs" | "permisos" | "analitica";

const TABS: { key: TabKey; label: string }[] = [
  { key: "config", label: "General" },
  { key: "docs", label: "Documentos" },
  { key: "permisos", label: "Permisos" },
  { key: "analitica", label: "Analítica" }
];

const STATUS_OPTIONS: StatusSelectOption<ChatbotLifecycleStatus>[] = (
  Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[]
).map((value) => ({ value, label: CHATBOT_STATUS_META[value].label, level: CHATBOT_STATUS_META[value].level }));

export function ChatbotDetailView() {
  const { key } = useParams<{ key: string }>();
  const { chatbots, updateStatus } = useChatbots();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialTab = (searchParams.get("tab") as TabKey | null) ?? "config";
  const [activeTab, setActiveTab] = useState<TabKey>(TABS.some((t) => t.key === initialTab) ? initialTab : "config");

  const bot = key ? chatbots[key] : undefined;

  if (!bot) {
    return (
      <section className="view active">
        <div className="glass" style={{ padding: 24 }}>
          <p>
            No se ha encontrado este chatbot.{" "}
            <button className="config-link" type="button" onClick={() => navigate("/chatbots")}>
              Volver a Chatbots →
            </button>
          </p>
        </div>
      </section>
    );
  }

  return (
    <section className="view active">
      <div className="bot-detail">
        <button className="btn-ghost back-btn" type="button" onClick={() => navigate("/chatbots")}>
          ← Chatbots
        </button>

        <div className="bot-detail-head">
          <div>
            <h2>{bot.label}</h2>
            <p>{botSubText(bot.docs.length, bot.team.length)}</p>
          </div>
          <StatusSelect
            value={bot.status}
            onChange={(status) => updateStatus(bot.key, status)}
            options={STATUS_OPTIONS}
            ariaLabel="Estado del chatbot"
          />
        </div>

        <div className="tabs" role="tablist" aria-label="Secciones del chatbot">
          {TABS.map((tab) => (
            <button
              key={tab.key}
              className="tab-btn"
              aria-selected={activeTab === tab.key}
              type="button"
              onClick={() => setActiveTab(tab.key)}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {activeTab === "config" && <GeneralTab bot={bot} />}
        {activeTab === "docs" && <DocumentsTab bot={bot} />}
        {activeTab === "permisos" && <PermissionsTab bot={bot} />}
        {activeTab === "analitica" && <AnalyticsTab bot={bot} />}
      </div>
    </section>
  );
}
