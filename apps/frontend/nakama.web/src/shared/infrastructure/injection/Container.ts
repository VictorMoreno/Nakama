import { MockChatbotsRepository } from "../../../modules/chatbots/infrastructure/MockChatbotsRepository";
import { MockConversationsRepository } from "../../../modules/chatbots/infrastructure/MockConversationsRepository";
import { MockFormationsRepository } from "../../../modules/formations/infrastructure/MockFormationsRepository";
import { MockUserRepository } from "../../../modules/user/infrastructure/MockUserRepository";
import { Services } from "./Services";

/**
 * Wires each injection token to its concrete implementation. Today every entry is a
 * Mock*Repository backed by in-memory seed data. Bringing up the real backend is a matter
 * of writing Api*Repository (implementing the same domain interface) and swapping the
 * entry below — nothing that calls useInjection needs to change.
 */
const registry = new Map<string, unknown>([
  [Services.ChatbotsRepository.id, new MockChatbotsRepository()],
  [Services.ConversationsRepository.id, new MockConversationsRepository()],
  [Services.FormationsRepository.id, new MockFormationsRepository()],
  [Services.UserRepository.id, new MockUserRepository()]
]);

export function resolve<T>(id: string): T {
  const service = registry.get(id);
  if (!service) throw new Error(`No service registered for "${id}"`);
  return service as T;
}
