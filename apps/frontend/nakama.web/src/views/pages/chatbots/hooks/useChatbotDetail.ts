import { useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";

export type TabKey = "config" | "docs" | "permisos" | "analitica";

export const TABS: { key: TabKey; label: string }[] = [
  { key: "config", label: "General" },
  { key: "docs", label: "Documentos" },
  { key: "permisos", label: "Permisos" },
  { key: "analitica", label: "Analítica" }
];

export function useChatbotDetail() {
  const { key } = useParams<{ key: string }>();
  const { chatbots, isAdmin, updateStatus } = useChatbots();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const initialTab = (searchParams.get("tab") as TabKey | null) ?? "config";
  const [activeTab, setActiveTab] = useState<TabKey>(TABS.some((t) => t.key === initialTab) ? initialTab : "config");

  const bot = key ? chatbots[key] : undefined;

  return { bot, isAdmin, updateStatus, navigate, activeTab, setActiveTab };
}
