import { AuthSession } from "./AuthSession";

export async function authFetch(input: RequestInfo, init?: RequestInit): Promise<Response> {
  const sentToken: string | null = AuthSession.getAccessToken();
  const headers: Headers = new Headers(init?.headers);
  if (sentToken) headers.set("Authorization", `Bearer ${sentToken}`);

  const response: Response = await fetch(input, { ...init, headers });

  if (response.status === 401 && sentToken && AuthSession.getAccessToken() === sentToken) {
    AuthSession.expire();
  }

  return response;
}
