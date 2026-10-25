import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { ConversationSummary } from "../../../../modules/chatbots/domain/Chatbot";
import type { IConversationsRepository } from "../../../../modules/chatbots/domain/IConversationsRepository";
import type { HomeFormation, HomeSummary } from "../../../../modules/home/domain/Home";
import type { IHomeRepository } from "../../../../modules/home/domain/IHomeRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useUser } from "../../../shared/contexts/UserContext";

function byProgressDescending(a: HomeFormation, b: HomeFormation): number {
  return b.progressPct - a.progressPct;
}

export function useHome() {
  const homeRepo = useInjection<IHomeRepository>(Services.HomeRepository.id);
  const conversationsRepo = useInjection<IConversationsRepository>(Services.ConversationsRepository.id);
  const { currentUser } = useUser();
  const { chatbots } = useChatbots();
  const navigate = useNavigate();

  const [summary, setSummary] = useState<HomeSummary | null>(null);
  const [failed, setFailed] = useState(false);
  const [latestConversation, setLatestConversation] = useState<ConversationSummary | null | undefined>(undefined);

  useEffect(() => {
    if (!currentUser) return;
    let cancelled = false;
    homeRepo
      .Find()
      .then((result) => {
        if (!cancelled) setSummary(result);
      })
      .catch(() => {
        if (!cancelled) setFailed(true);
      });
    conversationsRepo
      .FindLatest()
      .then((result) => {
        if (!cancelled) setLatestConversation(result);
      })
      .catch(() => {
        if (!cancelled) setLatestConversation(null);
      });
    return () => {
      cancelled = true;
    };
  }, [homeRepo, conversationsRepo, currentUser]);

  const formations = summary?.formations ?? [];
  const unfinished = formations.filter((formation) => !formation.completed);
  const started = unfinished.filter((formation) => formation.progressPct > 0).sort(byProgressDescending);
  const notStarted = unfinished.filter((formation) => formation.progressPct === 0);
  const continueFormation = started[0] ?? notStarted[0] ?? null;
  const otherFormations = unfinished.filter((formation) => formation !== continueFormation);

  return {
    firstName: currentUser?.name.split(" ")[0] ?? "",
    summary,
    failed,
    continueFormation,
    otherFormations,
    pendingExams: summary?.pendingExams ?? [],
    completedFormations: formations.filter((formation) => formation.completed).length,
    totalFormations: formations.length,
    latestConversation,
    latestConversationBot: latestConversation ? chatbots[latestConversation.botKey]?.label : undefined,
    navigate
  };
}
