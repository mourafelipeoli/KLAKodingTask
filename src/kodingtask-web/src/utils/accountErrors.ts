import { ApiError, NETWORK_ERROR } from "../services/httpClient";
import { textKey } from "../i18n/textKey";

// The refusals of the account, phone and delivery endpoints, by the server's code.
const MESSAGES: Readonly<Record<string, string>> = {
  MessagingUnavailable: textKey("This feature is not available here."),
  Paused: textKey("Sending is paused right now. Try again later."),
  DailyLimitReached: textKey(
    "Today's sending limit has been reached. Try again tomorrow.",
  ),
  MonthlyLimitReached: textKey(
    "This month's WhatsApp limit has been reached. Send it by email instead.",
  ),
  UserLimitReached: textKey("You have reached your sending limit for today."),
  SendFailed: textKey("The message could not be sent. Try again later."),
  EmailNotConfirmed: textKey("Confirm your email address first."),
  PhoneNotVerified: textKey(
    "Add and confirm your WhatsApp number in your fmID account, then choose WhatsApp again.",
  ),
  InvalidAntiforgeryToken: textKey(
    "Your session has changed. Reload the page and try again.",
  ),
  TooManyRequests: textKey("Too many requests. Wait a minute and try again."),
  [NETWORK_ERROR]: textKey(
    "The server is not reachable right now. Try again in a moment.",
  ),
};

const FALLBACK = textKey("The message could not be sent. Try again later.");

// Why a sign-in with fmID came back without a session (?error= on the sign-in page).
const FMID_INCOMPLETE = textKey("The sign-in with fmID did not complete. Try again.");

const FMID_MESSAGES: Readonly<Record<string, string>> = {
  "refused-by-provider": textKey(
    "fmID did not allow this sign-in. Check your fmID account and try again.",
  ),
  "invalid-response": FMID_INCOMPLETE,
  "no-access": textKey("Confirm your email address at fmID first, then try again."),
  unavailable: textKey("This feature is not available here."),
};

export function fmIdErrorKey(code: string | null): string | null {
  if (!code) {
    return null;
  }
  return FMID_MESSAGES[code] ?? FMID_INCOMPLETE;
}

export function accountErrorKey(error: unknown): string {
  const code = error instanceof ApiError ? error.code : NETWORK_ERROR;
  return MESSAGES[code] ?? FALLBACK;
}

/** Only paths on this site: anything else would let a crafted link send visitors away. */
export function safeReturnUrl(value: string | null): string {
  return value &&
    value.startsWith("/") &&
    !value.startsWith("//") &&
    !value.startsWith("/\\") &&
    !value.includes("://")
    ? value
    : "/";
}
