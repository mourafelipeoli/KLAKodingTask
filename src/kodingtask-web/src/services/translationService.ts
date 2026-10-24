import type { InterfaceLanguage, TranslationDictionary } from "../types/translation";
import { httpClient } from "./httpClient";

export const translationService = {
  languages: (signal?: AbortSignal) =>
    httpClient.get<InterfaceLanguage[]>("/api/i18n/languages", signal),
  /** The browser revalidates it with its ETag, so an unchanged dictionary costs a 304. */
  dictionary: (locale: string, signal?: AbortSignal) =>
    httpClient.get<TranslationDictionary>(
      `/api/i18n/${encodeURIComponent(locale)}`,
      signal,
    ),
  /** Texts the dictionaries do not know; sent in the background, even as the page closes. */
  register: (texts: readonly string[]) =>
    httpClient.sendInBackground("/api/i18n/terms", { texts }),
};
