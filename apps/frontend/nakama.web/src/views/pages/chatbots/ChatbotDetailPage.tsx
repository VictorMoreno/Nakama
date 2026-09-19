import StatusSelect, { type StatusSelectOption } from "../../components/shared/StatusSelect";
import { ChevronLeftIcon } from "../../components/shared/icons";
import { CHATBOT_STATUS_META } from "../../../lib/status";
import { botSubText } from "../../../lib/text";
import type { ChatbotLifecycleStatus } from "../../../modules/chatbots/domain/Chatbot";
import AnalyticsTab from "./tabs/AnalyticsTab";
import DocumentsTab from "./tabs/DocumentsTab";
import GeneralTab from "./tabs/GeneralTab";
import PermissionsTab from "./tabs/PermissionsTab";
import { TABS, useChatbotDetail } from "./hooks/useChatbotDetail";
import styles from "./ChatbotDetailPage.module.scss";

const STATUS_OPTIONS: StatusSelectOption<ChatbotLifecycleStatus>[] = (
  Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[]
).map((value) => ({ value, label: CHATBOT_STATUS_META[value].label, level: CHATBOT_STATUS_META[value].level }));

export default function ChatbotDetailPage() {
  const { bot, isAdmin, updateStatus, navigate, activeTab, setActiveTab } = useChatbotDetail();

  if (!bot) {
    return (
      <section className="view active">
        <div className="glass" style={{ padding: 24 }}>
          <p>
            No se ha encontrado este chatbot.{" "}
            <button className="config-link" type="button" onClick={() => navigate("/chatbots")}>
              Volver a Gestión de chatbots →
            </button>
          </p>
        </div>
      </section>
    );
  }

  return (
    <section className="view active">
      <div className={styles.botDetail}>
        <button className={`btn-ghost ${styles.backBtn}`} type="button" onClick={() => navigate("/chatbots")}>
          <ChevronLeftIcon /> Gestión de chatbots
        </button>

        <div className={styles.botDetailHead}>
          <div>
            <h2>{bot.label}</h2>
            <p>{botSubText(bot.docs.length, bot.team.length)}</p>
          </div>
          {isAdmin ? (
            <StatusSelect
              value={bot.status}
              onChange={(status) => updateStatus(bot.id, status)}
              options={STATUS_OPTIONS}
              ariaLabel="Estado del chatbot"
            />
          ) : (
            <span className={`status-tag ${CHATBOT_STATUS_META[bot.status].level}`}>{CHATBOT_STATUS_META[bot.status].label}</span>
          )}
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
