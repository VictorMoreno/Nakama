import { ApiHomeRepository } from "../../../modules/home/infrastructure/ApiHomeRepository";
import { ApiAnalyticsRepository } from "../../../modules/analytics/infrastructure/ApiAnalyticsRepository";
import { ApiAuthRepository } from "../../../modules/auth/infrastructure/ApiAuthRepository";
import { ApiChatbotsRepository } from "../../../modules/chatbots/infrastructure/ApiChatbotsRepository";
import { ApiConversationsRepository } from "../../../modules/chatbots/infrastructure/ApiConversationsRepository";
import { ApiFormationEnrollmentRepository } from "../../../modules/formations/infrastructure/ApiFormationEnrollmentRepository";
import { ApiFormationsRepository } from "../../../modules/formations/infrastructure/ApiFormationsRepository";
import { ApiLlmProvidersRepository } from "../../../modules/llmProviders/infrastructure/ApiLlmProvidersRepository";
import { ApiUsersRepository } from "../../../modules/users/infrastructure/ApiUsersRepository";
import { authFetch } from "../auth/authFetch";
import { GeneratedNakamaClient } from "../GeneratedNakamaClient";
import { Services } from "./Services";

const client = new GeneratedNakamaClient(import.meta.env.VITE_API_BASE_URL ?? "", { fetch: authFetch });

const registry = new Map<string, unknown>([
  [Services.AnalyticsRepository.id, new ApiAnalyticsRepository(client)],
  [Services.AuthRepository.id, new ApiAuthRepository(client)],
  [Services.ChatbotsRepository.id, new ApiChatbotsRepository(client)],
  [Services.ConversationsRepository.id, new ApiConversationsRepository(client)],
  [Services.FormationsRepository.id, new ApiFormationsRepository(client)],
  [Services.FormationEnrollmentRepository.id, new ApiFormationEnrollmentRepository(client)],
  [Services.HomeRepository.id, new ApiHomeRepository(client)],
  [Services.LlmProvidersRepository.id, new ApiLlmProvidersRepository(client)],
  [Services.UsersRepository.id, new ApiUsersRepository(client)]
]);

export function resolve<T>(id: string): T {
  const service = registry.get(id);
  if (!service) throw new Error(`No service registered for "${id}"`);
  return service as T;
}
