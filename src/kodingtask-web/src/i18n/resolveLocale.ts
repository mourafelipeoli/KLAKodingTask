import type { LocaleCode } from "../config/locales";
import { DEFAULT_LOCALE, matchLocale } from "../config/locales";
import type { InterfaceLanguage } from "../types/translation";

// The server reads the same cookie, so its error messages and emails follow the page.
const COOKIE = "kodingtask-lang";
const ONE_YEAR_SECONDS = 365 * 24 * 60 * 60;

// Order (the server's too): an explicit ?lang= in the address, the cookie, the browser's
// preferred languages, then English.
export function resolveInitialLocale(
  languages: readonly InterfaceLanguage[],
): LocaleCode {
  const fromAddress = matchLocale(
    new URLSearchParams(window.location.search).get("lang"),
    languages,
  );
  if (fromAddress) {
    return fromAddress;
  }
  const fromCookie = matchLocale(readLocaleCookie(), languages);
  if (fromCookie) {
    return fromCookie;
  }
  for (const language of navigator.languages) {
    const match = matchLocale(language, languages);
    if (match) {
      return match;
    }
  }
  return DEFAULT_LOCALE;
}

export function readLocaleCookie(): string | null {
  const entry = document.cookie
    .split(";")
    .map((part) => part.trim())
    .find((part) => part.startsWith(`${COOKIE}=`));
  return entry ? decodeURIComponent(entry.slice(COOKIE.length + 1)) : null;
}

/** A choice made in the menu lasts a year; a detected language only this browser session. */
export function storeLocale(locale: LocaleCode, isChoice: boolean): void {
  const lifetime = isChoice ? `; max-age=${ONE_YEAR_SECONDS}` : "";
  const secure = window.location.protocol === "https:" ? "; secure" : "";
  document.cookie = `${COOKIE}=${encodeURIComponent(locale)}; path=/; samesite=lax${lifetime}${secure}`;
}
