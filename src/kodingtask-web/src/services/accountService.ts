import type { Account, PhoneCodeResult } from "../types/account";
import { httpClient } from "./httpClient";

export const accountService = {
  antiforgery: () => httpClient.get<{ token: string }>("/api/account/antiforgery"),
  me: (signal?: AbortSignal) => httpClient.get<Account>("/api/account/me", signal),
  /** Leaves for fmID; the browser comes back to returnUrl signed in. */
  signIn: (returnUrl: string, uiLocale: string) =>
    httpClient.navigateWithPost("/api/account/fmid/sign-in", { returnUrl, uiLocale }),
  /** Ends the session here and at fmID, then comes back to returnUrl. */
  signOut: (returnUrl: string) =>
    httpClient.navigateWithPost("/api/account/fmid/sign-out", { returnUrl }),
  sendPhoneCode: (phone: string, uiLocale: string) =>
    httpClient.post<PhoneCodeResult>("/api/account/phone/code", { phone, uiLocale }),
  verifyPhone: (code: string) =>
    httpClient.post<Account>("/api/account/phone/verify", { code }),
};
