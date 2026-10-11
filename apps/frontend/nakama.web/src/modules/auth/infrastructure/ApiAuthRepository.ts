import { AuthSession } from "../../../shared/infrastructure/auth/AuthSession";
import { GeneratedApiException, type GeneratedNakamaClient, type LoginResponse } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { AuthenticatedUser } from "../domain/AuthenticatedUser";
import type { IAuthRepository } from "../domain/IAuthRepository";

const HTTP_UNAUTHORIZED = 401;

function isUnauthorized(error: unknown): boolean {
  if (GeneratedApiException.isGeneratedApiException(error)) return error.status === HTTP_UNAUTHORIZED;
  return typeof error === "object" && error !== null && "status" in error && (error as { status: unknown }).status === HTTP_UNAUTHORIZED;
}

export class ApiAuthRepository implements IAuthRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async Login(email: string, password: string): Promise<AuthenticatedUser | null> {
    let response: LoginResponse;
    try {
      response = await this.client.login({ email, password });
    } catch (error) {
      if (isUnauthorized(error)) return null;
      throw error;
    }

    if (!response.accessToken || !response.user) {
      throw new Error("La respuesta de inicio de sesión no incluye credenciales.");
    }

    AuthSession.setAccessToken(response.accessToken);
    const user = response.user;
    return { id: user.id ?? "", email: user.email ?? "", name: user.name ?? "", isAdmin: user.isAdmin ?? false };
  }

  async Logout(): Promise<void> {
    AuthSession.clear();
  }
}
