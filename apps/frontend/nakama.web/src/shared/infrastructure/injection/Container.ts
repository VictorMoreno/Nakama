import { ApiAnalyticsRepository } from "../../../modules/analytics/infrastructure/ApiAnalyticsRepository";
import { ApiChatbotsRepository } from "../../../modules/chatbots/infrastructure/ApiChatbotsRepository";
import { ApiConversationsRepository } from "../../../modules/chatbots/infrastructure/ApiConversationsRepository";
import { ApiFormationsRepository } from "../../../modules/formations/infrastructure/ApiFormationsRepository";
import { MockUserRepository } from "../../../modules/user/infrastructure/MockUserRepository";
import { GeneratedNakamaClient } from "../GeneratedNakamaClient";
import { Services } from "./Services";

/**
 * Wires each injection token to its concrete implementation. Analytics/Chatbots/Conversations/
 * Formations are backed by the real API through one shared GeneratedNakamaClient instance
 * (empty base URL in dev - vite.config.ts already proxies /api/* to the local API; set
 * VITE_API_BASE_URL for a build targeting a real deployed origin). User has no backend
 * bounded context yet (out of scope for now), so it stays mocked until a real auth/session
 * API exists.
 */
const client = new GeneratedNakamaClient(import.meta.env.VITE_API_BASE_URL ?? "");

const registry = new Map<string, unknown>([
  [Services.AnalyticsRepository.id, new ApiAnalyticsRepository(client)],
  [Services.ChatbotsRepository.id, new ApiChatbotsRepository(client)],
  [Services.ConversationsRepository.id, new ApiConversationsRepository(client)],
  [Services.FormationsRepository.id, new ApiFormationsRepository(client)],
  [Services.UserRepository.id, new MockUserRepository()]
]);

export function resolve<T>(id: string): T {
  const service = registry.get(id);
  if (!service) throw new Error(`No service registered for "${id}"`);
  return service as T;
}
