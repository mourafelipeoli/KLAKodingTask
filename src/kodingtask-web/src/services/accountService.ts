import type { Account } from "../types/account";
import { httpClient } from "./httpClient";

export const accountService = {
  antiforgery: () => httpClient.get<{ token: string }>("/api/account/antiforgery"),
  me: (signal?: AbortSignal) => httpClient.get<Account>("/api/account/me", signal),
  /**
   * Leaves for fmID; the browser comes back to returnUrl signed in. `isRetry` marks the
   * automatic second attempt after a sign-in that finished in another browser, so the
   * server never retries twice.
   */
  signIn: (returnUrl: string, uiLocale: string, isRetry = false) =>
    httpClient.navigateWithPost("/api/account/fmid/sign-in", {
      returnUrl,
      uiLocale,
      ...(isRetry ? { retry: "1" } : {}),
    }),
  /**
   * Asks fmID again without showing anything when its session is still open, to pick up a
   * WhatsApp number added or changed there; comes back to returnUrl.
   */
  refresh: (returnUrl: string, uiLocale: string) =>
    httpClient.navigateWithPost("/api/account/fmid/refresh", { returnUrl, uiLocale }),
  /** Ends the session here and at fmID, then comes back to returnUrl. */
  signOut: (returnUrl: string) =>
    httpClient.navigateWithPost("/api/account/fmid/sign-out", { returnUrl }),
};
