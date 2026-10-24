/** One interface language, as the server's InterfaceLanguage enum declares it. */
export interface InterfaceLanguage {
  code: string;
  nativeName: string;
  /** A regional variant only holds what differs from this language (fr-CA from fr-FR). */
  variantOf: string | null;
}

/** Every term the server knows, in one language; English maps each term to itself. */
export interface TranslationDictionary {
  locale: string;
  /** Changes whenever a text changes; the server also sends it as the ETag. */
  version: string;
  entries: Record<string, string>;
}

export const TranslationSource = {
  Baked: "Baked",
  Machine: "Machine",
  Manual: "Manual",
} as const;

export type TranslationSource =
  (typeof TranslationSource)[keyof typeof TranslationSource];

/** One term on the review page, with its translation into the language under review. */
export interface AdminTranslationRow {
  termId: number;
  english: string;
  /** Code, Visitor (sent by a page) or Server (used by the server itself). */
  origin: string;
  text: string | null;
  /** For a regional variant: what it shows when it has no text of its own. */
  inherited: string | null;
  source: TranslationSource | null;
  reviewed: boolean;
  /** ISO 8601 with offset. */
  updatedAt: string | null;
}

export interface AdminTranslationUpdate {
  termId: number;
  locale: string;
  /** Empty removes a regional override, so the variant inherits again. */
  text: string;
  reviewed: boolean;
}
