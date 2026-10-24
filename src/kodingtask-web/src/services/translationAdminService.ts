import type { AdminTranslationRow, AdminTranslationUpdate } from "../types/translation";
import { httpClient } from "./httpClient";

export const translationAdminService = {
  list: (locale: string, signal?: AbortSignal) =>
    httpClient.get<AdminTranslationRow[]>(
      `/api/admin/translations?locale=${encodeURIComponent(locale)}`,
      signal,
    ),
  update: (update: AdminTranslationUpdate) =>
    httpClient.put<AdminTranslationRow>("/api/admin/translations", update),
  /** Wakes the machine translation up instead of waiting for its next round. */
  run: () => httpClient.post<void>("/api/admin/translations/run", {}),
  /** A download in the bundled dictionaries' format, ready to replace the file in the repository. */
  exportUrl: (locale: string) =>
    `/api/admin/translations/export/${encodeURIComponent(locale)}`,
};
