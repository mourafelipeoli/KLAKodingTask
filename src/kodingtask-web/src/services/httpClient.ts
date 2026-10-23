// The single HTTP client of the web app. Services call it; components never call fetch.
// It turns every failure into an ApiError carrying the server's error code, so screens
// can show a translated message instead of a raw status.

export const NETWORK_ERROR = "NetworkError";

export class ApiError extends Error {
  readonly status: number;
  readonly code: string;

  constructor(status: number, code: string, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
  }
}

interface ErrorBody {
  error?: string;
  message?: string;
  title?: string;
}

export function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === "AbortError";
}

async function toApiError(response: Response): Promise<ApiError> {
  let body: ErrorBody = {};
  try {
    body = (await response.json()) as ErrorBody;
  } catch {
    // Not JSON (a proxy error page, for instance): keep the status only.
  }
  const code =
    body.error ??
    (response.status === 429 ? "TooManyRequests" : `Http${response.status}`);
  return new ApiError(
    response.status,
    code,
    body.message ?? body.title ?? response.statusText,
  );
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
  signal?: AbortSignal,
): Promise<T> {
  const headers: Record<string, string> = { Accept: "application/json" };
  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  let response: Response;
  try {
    response = await fetch(path, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      credentials: "same-origin",
      signal,
    });
  } catch (error) {
    if (isAbortError(error)) {
      throw error;
    }
    throw new ApiError(0, NETWORK_ERROR, "The server could not be reached.");
  }

  if (!response.ok) {
    throw await toApiError(response);
  }
  return (await response.json()) as T;
}

export const httpClient = {
  get: <T>(path: string, signal?: AbortSignal) =>
    request<T>("GET", path, undefined, signal),
  post: <T>(path: string, body: unknown, signal?: AbortSignal) =>
    request<T>("POST", path, body, signal),
};
