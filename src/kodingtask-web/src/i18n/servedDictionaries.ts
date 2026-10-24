import type { LocaleCode } from "../config/locales";
import type { TranslationDictionary } from "../types/translation";
import type { Dictionary } from "./dictionaries";

// The last dictionary the server sent for each language, kept in the browser so a reload
// paints translated text at once, also in a language that has no bundled dictionary. The
// server's current copy replaces it as soon as it arrives.
const PREFIX = "kodingtask.dictionary.";

export function readServedDictionary(locale: LocaleCode): Dictionary | null {
  try {
    const stored = window.localStorage.getItem(PREFIX + locale);
    if (!stored) {
      return null;
    }
    const dictionary = JSON.parse(stored) as TranslationDictionary;
    return dictionary.locale === locale ? dictionary.entries : null;
  } catch {
    return null;
  }
}

export function keepServedDictionary(dictionary: TranslationDictionary): void {
  try {
    window.localStorage.setItem(PREFIX + dictionary.locale, JSON.stringify(dictionary));
  } catch {
    // Private windows and full or blocked storage: the next visit loads it again.
  }
}
