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
  { code: "zh-CN", nativeName: "简体中文" },
  { code: "zh-TW", nativeName: "繁體中文" },
  { code: "ja-JP", nativeName: "日本語" },
  { code: "ko-KR", nativeName: "한국어" },
] as const;

export type LocaleCode = (typeof LOCALES)[number]["code"];

export const DEFAULT_LOCALE: LocaleCode = "en-US";

// A regional French dictionary only holds what differs from France; the rest falls back.
export const PARENT_LOCALE: Partial<Record<LocaleCode, LocaleCode>> = {
  "fr-CA": "fr-FR",
  "fr-CH": "fr-FR",
  "fr-BE": "fr-FR",
};

// Closest supported locale for a language without an exact match (de-AT, pt-PT, en-GB, zh-SG…).
const BY_LANGUAGE: Record<string, LocaleCode> = {
  en: "en-US",
  pt: "pt-BR",
  de: "de-DE",
  fr: "fr-FR",
  it: "it-IT",
  zh: "zh-CN",
  ja: "ja-JP",
  ko: "ko-KR",
};

// Chinese written in traditional characters: Taiwan, Hong Kong, Macau, or an explicit script.
const TRADITIONAL_CHINESE = /^zh-(tw|hk|mo|hant)(-|$)/;

export function matchLocale(candidate: string | null | undefined): LocaleCode | null {
  if (!candidate) {
    return null;
  }
  const normalized = candidate.replace("_", "-").toLowerCase();
  const exact = LOCALES.find((locale) => locale.code.toLowerCase() === normalized);
  if (exact) {
    return exact.code;
  }
  if (TRADITIONAL_CHINESE.test(normalized)) {
    return "zh-TW";
  }
  const language = normalized.split("-")[0] ?? "";
  return BY_LANGUAGE[language] ?? null;
}
