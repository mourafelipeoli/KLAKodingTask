// The UI locales. They are the same codes as the server's output languages, so one code
// means one thing everywhere: "fr-BE" is Belgian French in the menu and in the words.
export const LOCALES = [
  { code: "en-US", nativeName: "English (US)" },
  { code: "pt-BR", nativeName: "Português (Brasil)" },
  { code: "de-DE", nativeName: "Deutsch (Deutschland)" },
  { code: "fr-FR", nativeName: "Français (France)" },
  { code: "fr-CA", nativeName: "Français (Canada)" },
  { code: "fr-CH", nativeName: "Français (Suisse)" },
  { code: "fr-BE", nativeName: "Français (Belgique)" },
  { code: "it-IT", nativeName: "Italiano (Italia)" },
] as const;

export type LocaleCode = (typeof LOCALES)[number]["code"];

export const DEFAULT_LOCALE: LocaleCode = "en-US";

// A regional French dictionary only holds what differs from France; the rest falls back.
export const PARENT_LOCALE: Partial<Record<LocaleCode, LocaleCode>> = {
  "fr-CA": "fr-FR",
  "fr-CH": "fr-FR",
  "fr-BE": "fr-FR",
};

// Closest supported locale for a language without an exact match (de-AT, pt-PT, en-GB…).
const BY_LANGUAGE: Record<string, LocaleCode> = {
  en: "en-US",
  pt: "pt-BR",
  de: "de-DE",
  fr: "fr-FR",
  it: "it-IT",
};

export function matchLocale(candidate: string | null | undefined): LocaleCode | null {
  if (!candidate) {
    return null;
  }
  const normalized = candidate.replace("_", "-").toLowerCase();
  const exact = LOCALES.find((locale) => locale.code.toLowerCase() === normalized);
  if (exact) {
    return exact.code;
  }
  const language = normalized.split("-")[0] ?? "";
  return BY_LANGUAGE[language] ?? null;
}
