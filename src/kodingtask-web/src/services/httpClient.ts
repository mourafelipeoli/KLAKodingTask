// The single HTTP client of the web app. Services call it; components never call fetch.
// It turns every failure into an ApiError carrying the server's error code, so screens
// can show a translated message instead of a raw status.

export const NETWORK_ERROR = "NetworkError";

// The antiforgery token the server issued for the current session, sent with every
// state-changing request. Fetched again whenever the session changes (sign in, sign out),
// and fetched first if a state-changing request starts before the app has one (a page
// opened from an email link posts right away).
let antiforgeryToken: string | null = null;
let antiforgeryRequest: Promise<void> | null = null;

export function setAntiforgeryToken(token: string | null): void {
  antiforgeryToken = token;
}

async function ensureAntiforgeryToken(): Promise<void> {
  if (antiforgeryToken) {
    return;
  }
  antiforgeryRequest ??= fetch("/api/account/antiforgery", {
    headers: { Accept: "application/json" },
    credentials: "same-origin",
  })
    .then(async (response) => {
      if (response.ok) {
        antiforgeryToken = ((await response.json()) as { token: string }).token;
      }
    })
    .catch(() => {
      // The request itself will fail and report the network error.
    })
    .finally(() => {
      antiforgeryRequest = null;
    });
  await antiforgeryRequest;
}

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
  if (method !== "GET") {
    await ensureAntiforgeryToken();
    if (antiforgeryToken) {
      headers["X-XSRF-TOKEN"] = antiforgeryToken;
    }
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
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

// Sign-in and sign-out with fmID leave the site, so they are form posts the browser follows
// rather than fetch calls; the antiforgery token travels as a form field.
async function navigateWithPost(
  path: string,
  fields: Record<string, string>,
): Promise<void> {
  await ensureAntiforgeryToken();
  const form = document.createElement("form");
  form.method = "post";
  form.action = path;
  const values = { ...fields, __RequestVerificationToken: antiforgeryToken ?? "" };
  for (const [name, value] of Object.entries(values)) {
    const input = document.createElement("input");
    input.type = "hidden";
    input.name = name;
    input.value = value;
    form.append(input);
  }
  document.body.append(form);
  form.submit();
}

export const httpClient = {
  get: <T>(path: string, signal?: AbortSignal) =>
    request<T>("GET", path, undefined, signal),
  post: <T>(path: string, body: unknown, signal?: AbortSignal) =>
    request<T>("POST", path, body, signal),
  delete: <T>(path: string, signal?: AbortSignal) =>
    request<T>("DELETE", path, undefined, signal),
  navigateWithPost,
};
