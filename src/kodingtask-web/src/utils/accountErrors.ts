import { ApiError, NETWORK_ERROR } from "../services/httpClient";
import { textKey } from "../i18n/textKey";

// The refusals of the account, phone and delivery endpoints, by the server's code.
const MESSAGES: Readonly<Record<string, string>> = {
  MessagingUnavailable: textKey("This feature is not available here."),
  Paused: textKey("Sending is paused right now. Try again later."),
  DailyLimitReached: textKey(
    "Today's sending limit has been reached. Try again tomorrow.",
  ),
  UserLimitReached: textKey("You have reached your sending limit for today."),
  SendFailed: textKey("The message could not be sent. Try again later."),
  InvalidCredentials: textKey("The email or the password is not right."),
  LockedOut: textKey("Too many failed attempts. Try again in 15 minutes."),
  WeakPassword: textKey("Use a password of at least 10 characters."),
  InvalidEmail: textKey("Type a valid email address."),
  EmailTaken: textKey(
    "An account with this email already exists. Sign in or reset the password.",
  ),
  RegistrationLimitReached: textKey(
    "Too many accounts were created from this connection today.",
  ),
  InvalidLink: textKey("This link is invalid or has expired."),
  EmailNotConfirmed: textKey("Confirm your email address first."),
  PhoneNotVerified: textKey("Verify your WhatsApp number first."),
  InvalidPhone: textKey(
    "Type the number in international format, for example +49 170 1234567.",
  ),
  CodeCooldown: textKey("Wait a minute before asking for a new code."),
  NoPendingCode: textKey("Ask for a code first."),
  CodeExpired: textKey("The code has expired. Ask for a new one."),
  WrongCode: textKey("The code is not right."),
  TooManyAttempts: textKey("Too many wrong codes. Ask for a new one."),
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
  "registration-limit": textKey(
    "Too many accounts were created from this connection today.",
  ),
  paused: textKey("New accounts are paused right now. Try again later."),
  "locked-out": textKey("Too many failed attempts. Try again in 15 minutes."),
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
