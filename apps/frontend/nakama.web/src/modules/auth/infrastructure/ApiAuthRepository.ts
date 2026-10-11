import { AuthSession } from "../../../shared/infrastructure/auth/AuthSession";
import { GeneratedApiException, type GeneratedNakamaClient, type TokenResponse } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { AuthenticatedUser } from "../domain/AuthenticatedUser";
import type { IAuthRepository } from "../domain/IAuthRepository";

const HTTP_UNAUTHORIZED = 401;

function isUnauthorized(error: unknown): boolean {
  if (GeneratedApiException.isGeneratedApiException(error)) return error.status === HTTP_UNAUTHORIZED;
  return typeof error === "object" && error !== null && "status" in error && (error as { status: unknown }).status === HTTP_UNAUTHORIZED;
}

function toAuthenticatedUser(response: TokenResponse): AuthenticatedUser {
  if (!response.accessToken || !response.user) {
    throw new Error("La respuesta de autenticación no incluye credenciales.");
  }

  AuthSession.setAccessToken(response.accessToken);
  const user = response.user;
  return { id: user.id ?? "", email: user.email ?? "", name: user.name ?? "", isAdmin: user.isAdmin ?? false };
}

export class ApiAuthRepository implements IAuthRepository {
  private readonly client: GeneratedNakamaClient;
  private pendingRefresh: Promise<AuthenticatedUser | null> | null = null;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
    AuthSession.setRefresher(() =>
      this.Refresh().then(
        (user) => user !== null,
        () => false
      )
    );
  }

  async Login(email: string, password: string): Promise<AuthenticatedUser | null> {
    let response: TokenResponse;
    try {
      response = await this.client.login({ email, password });
    } catch (error) {
      if (isUnauthorized(error)) return null;
      throw error;
    }

    return toAuthenticatedUser(response);
  }

  Refresh(): Promise<AuthenticatedUser | null> {
    this.pendingRefresh ??= this.requestRefresh().finally(() => {
      this.pendingRefresh = null;
    });
    return this.pendingRefresh;
  }

  async Logout(): Promise<void> {
    await this.client.logout().catch(() => undefined);
    AuthSession.clear();
  }

  private async requestRefresh(): Promise<AuthenticatedUser | null> {
    try {
      return toAuthenticatedUser(await this.client.refresh());
    } catch (error) {
      if (isUnauthorized(error)) return null;
      throw error;
    }
  }
}
