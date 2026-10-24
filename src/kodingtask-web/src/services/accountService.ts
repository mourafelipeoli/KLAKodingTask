import type { Account, PhoneCodeResult, RegisterResult } from "../types/account";
import { httpClient } from "./httpClient";

export const accountService = {
  antiforgery: () => httpClient.get<{ token: string }>("/api/account/antiforgery"),
  me: (signal?: AbortSignal) => httpClient.get<Account>("/api/account/me", signal),
  register: (email: string, password: string, uiLocale: string, returnUrl: string) =>
    httpClient.post<RegisterResult>("/api/account/register", {
      email,
      password,
      uiLocale,
      returnUrl,
    }),
  confirmEmail: (userId: string, code: string) =>
    httpClient.post<Account>("/api/account/confirm-email", { userId, code }),
  resendConfirmation: (uiLocale: string, returnUrl: string) =>
    httpClient.post<Account>("/api/account/resend-confirmation", { uiLocale, returnUrl }),
  signIn: (email: string, password: string) =>
    httpClient.post<Account>("/api/account/sign-in", { email, password }),
  signOut: () => httpClient.post<Account>("/api/account/sign-out", {}),
  /** Leaves for fmID; the browser comes back to returnUrl signed in. */
  startFmIdSignIn: (returnUrl: string, uiLocale: string) =>
    httpClient.navigateWithPost("/api/account/fmid/sign-in", { returnUrl, uiLocale }),
  /** Ends the session here and at fmID, then comes back to returnUrl. */
  signOutOfFmId: (returnUrl: string) =>
    httpClient.navigateWithPost("/api/account/fmid/sign-out", { returnUrl }),
  forgotPassword: (email: string, uiLocale: string) =>
    httpClient.post<undefined>("/api/account/forgot-password", { email, uiLocale }),
  resetPassword: (userId: string, code: string, password: string) =>
    httpClient.post<Account>("/api/account/reset-password", { userId, code, password }),
  sendPhoneCode: (phone: string, uiLocale: string) =>
    httpClient.post<PhoneCodeResult>("/api/account/phone/code", { phone, uiLocale }),
  verifyPhone: (code: string) =>
    httpClient.post<Account>("/api/account/phone/verify", { code }),
};
