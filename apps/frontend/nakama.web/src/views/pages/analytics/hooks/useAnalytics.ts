import { useChatbots } from "../../../shared/contexts/ChatbotsContext";

export function useAnalytics() {
  const { chatbots } = useChatbots();

  const globalGaps = Object.values(chatbots)
    .filter((bot) => !bot.archived)
    .flatMap((bot) => bot.gaps.map((g) => ({ ...g, dept: bot.label })))
    .sort((a, b) => b.count - a.count);

  return { chatbots, globalGaps };
}
