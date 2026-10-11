import { AuthSession } from "./AuthSession";

const AUTH_ENDPOINTS_PATH = "/api/auth/";

function isAuthEndpoint(input: RequestInfo): boolean {
  const url: string = typeof input === "string" ? input : input.url;
  return url.includes(AUTH_ENDPOINTS_PATH);
}

function send(input: RequestInfo, init: RequestInit | undefined, token: string | null): Promise<Response> {
  const headers: Headers = new Headers(init?.headers);
  if (token) headers.set("Authorization", `Bearer ${token}`);
  return fetch(input, { ...init, headers });
}

export async function authFetch(input: RequestInfo, init?: RequestInit): Promise<Response> {
  const sentToken: string | null = AuthSession.getAccessToken();
  const response: Response = await send(input, init, sentToken);

  if (response.status !== 401 || !sentToken || isAuthEndpoint(input)) return response;

  const currentToken: string | null = AuthSession.getAccessToken();
  const renewed: boolean = currentToken !== null && currentToken !== sentToken ? true : await AuthSession.refresh();
  if (!renewed) {
    AuthSession.expire();
    return response;
  }

  return send(input, init, AuthSession.getAccessToken());
}
