import type { ApiErrorBody, FieldError } from "./types";

/** A failed API call. `code` is the backend's stable code, `UNKNOWN`, or `NETWORK_ERROR` (status 0). */
export class ApiError extends Error {
  readonly status: number;
  readonly code: string;
  readonly fieldErrors: FieldError[];

  constructor(status: number, code: string, message?: string, fieldErrors: FieldError[] = []) {
    super(message ?? `Request failed with status ${status} (${code})`);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.fieldErrors = fieldErrors;
  }
}

const LOGIN_PATH = "/api/auth/login";
const SESSION_EXPIRED = "session-expired";
const sessionEvents = new EventTarget();

/**
 * Subscribe to "session expired": any 401 except the login call itself.
 * Returns the unsubscribe function.
 */
export function onSessionExpired(listener: () => void): () => void {
  sessionEvents.addEventListener(SESSION_EXPIRED, listener);
  return () => sessionEvents.removeEventListener(SESSION_EXPIRED, listener);
}

function isLoginCall(path: string): boolean {
  return path.split("?")[0].toLowerCase() === LOGIN_PATH;
}

async function readError(response: Response): Promise<ApiError> {
  let body: ApiErrorBody | undefined;
  try {
    const parsed: unknown = await response.json();
    if (parsed && typeof parsed === "object") body = parsed as ApiErrorBody;
  } catch {
    body = undefined;
  }
  const code = typeof body?.code === "string" && body.code ? body.code : "UNKNOWN";
  const fieldErrors = Array.isArray(body?.errors) ? body.errors : [];
  return new ApiError(response.status, code, body?.message, fieldErrors);
}

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const hasBody = body !== undefined;
  const init: RequestInit = {
    method,
    credentials: "same-origin",
    headers: hasBody ? { "Content-Type": "application/json" } : {},
    body: hasBody ? JSON.stringify(body) : undefined,
  };

  let response: Response;
  try {
    response = await fetch(path, init);
  } catch (cause) {
    const message = cause instanceof Error ? cause.message : "Network request failed";
    throw new ApiError(0, "NETWORK_ERROR", message);
  }

  if (!response.ok) {
    const error = await readError(response);
    if (response.status === 401 && !isLoginCall(path)) {
      sessionEvents.dispatchEvent(new Event(SESSION_EXPIRED));
    }
    throw error;
  }

  if (response.status === 204) return undefined as T;
  const text = await response.text();
  return (text ? JSON.parse(text) : undefined) as T;
}

export const api = {
  get: <T>(path: string) => request<T>("GET", path),
  post: <T>(path: string, body?: unknown) => request<T>("POST", path, body),
  put: <T>(path: string, body: unknown) => request<T>("PUT", path, body),
  delete: (path: string) => request<void>("DELETE", path),
};
