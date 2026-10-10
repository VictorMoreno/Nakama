import { GeneratedApiException, type GeneratedNakamaClient } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { AuthenticatedUser } from "../domain/AuthenticatedUser";
import type { IAuthRepository } from "../domain/IAuthRepository";

const HTTP_UNAUTHORIZED = 401;

export class ApiAuthRepository implements IAuthRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async Login(email: string, password: string): Promise<AuthenticatedUser | null> {
    try {
      const user = await this.client.login({ email, password });
      return { id: user.id ?? "", email: user.email ?? "", name: user.name ?? "", isAdmin: user.isAdmin ?? false };
    } catch (error) {
      if (GeneratedApiException.isGeneratedApiException(error) && error.status === HTTP_UNAUTHORIZED) return null;
      if (isProblemDetails(error) && error.status === HTTP_UNAUTHORIZED) return null;
      throw error;
    }
  }
}

function isProblemDetails(error: unknown): error is { status: number } {
  return typeof error === "object" && error !== null && "status" in error && typeof (error as { status: unknown }).status === "number";
}
