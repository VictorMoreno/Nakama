import { ApiServerError } from "../errors/ApiServerError";
import { ServerErrorSession } from "../errors/ServerErrorSession";
import { AuthSession } from "./AuthSession";

const AUTH_ENDPOINTS_PATH = "/api/auth/";
const LOGOUT_ENDPOINT_PATH = "/api/auth/logout";
const HTTP_SERVER_ERROR = 500;

function urlOf(input: RequestInfo): string {
  return typeof input === "string" ? input : input.url;
}

function isAuthEndpoint(input: RequestInfo): boolean {
  return urlOf(input).includes(AUTH_ENDPOINTS_PATH);
}

function reportsServerErrors(input: RequestInfo): boolean {
  return !urlOf(input).includes(LOGOUT_ENDPOINT_PATH);
}

async function send(input: RequestInfo, init: RequestInit | undefined, token: string | null): Promise<Response> {
  const headers: Headers = new Headers(init?.headers);
  if (token) headers.set("Authorization", `Bearer ${token}`);

  let response: Response;
  try {
    response = await fetch(input, { ...init, headers });
  } catch (error) {
    if (error instanceof TypeError && reportsServerErrors(input)) {
      ServerErrorSession.report(new ApiServerError("No se pudo conectar con el servidor.", null));
    }
    throw error;
  }

  if (response.status >= HTTP_SERVER_ERROR && reportsServerErrors(input)) {
    ServerErrorSession.report(new ApiServerError(`El servidor respondió con un error (HTTP ${response.status}).`, response.status));
  }

  return response;
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
