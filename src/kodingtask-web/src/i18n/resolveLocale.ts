import type { LocaleCode } from "../config/locales";
import { DEFAULT_LOCALE, matchLocale } from "../config/locales";

const STORAGE_KEY = "kodingtask.locale";

// Order: an explicit ?lang= in the address, the visitor's last choice, the browser's
// preferred languages, then English.
export function resolveInitialLocale(): LocaleCode {
  const fromAddress = matchLocale(
    new URLSearchParams(window.location.search).get("lang"),
  );
  if (fromAddress) {
    return fromAddress;
  }
  const fromStorage = matchLocale(readStoredLocale());
  if (fromStorage) {
    return fromStorage;
  }
  for (const language of navigator.languages) {
    const match = matchLocale(language);
    if (match) {
      return match;
    }
  }
  return DEFAULT_LOCALE;
}

export function storeLocale(locale: LocaleCode): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, locale);
  } catch {
    // Private windows and blocked storage: the choice simply lasts until the tab closes.
  }
}

function readStoredLocale(): string | null {
  try {
    return window.localStorage.getItem(STORAGE_KEY);
  } catch {
    return null;
  }
}
