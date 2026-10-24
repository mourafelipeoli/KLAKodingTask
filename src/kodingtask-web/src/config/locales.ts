import type { InterfaceLanguage } from "../types/translation";

// The interface languages. The server's InterfaceLanguage enum is the list: a language added
// there appears in the menu and gets its texts without any change here. This copy matches
// the dictionaries bundled with the app and is used until the server's list arrives (and if
// it never does). The codes are also the converter's output languages, so one code means one
// thing everywhere: "fr-BE" is Belgian French in the menu and in the words.
export const LOCALES: readonly InterfaceLanguage[] = [
  { code: "en-US", nativeName: "English (US)", variantOf: null },
  { code: "pt-BR", nativeName: "Português (Brasil)", variantOf: null },
  { code: "de-DE", nativeName: "Deutsch (Deutschland)", variantOf: null },
  { code: "fr-FR", nativeName: "Français (France)", variantOf: null },
  { code: "fr-CA", nativeName: "Français (Canada)", variantOf: "fr-FR" },
  { code: "fr-CH", nativeName: "Français (Suisse)", variantOf: "fr-FR" },
  { code: "fr-BE", nativeName: "Français (Belgique)", variantOf: "fr-FR" },
  { code: "it-IT", nativeName: "Italiano (Italia)", variantOf: null },
  { code: "es-ES", nativeName: "Español (España)", variantOf: null },
  { code: "pl-PL", nativeName: "Polski (Polska)", variantOf: null },
  { code: "zh-CN", nativeName: "简体中文", variantOf: null },
  { code: "zh-TW", nativeName: "繁體中文", variantOf: null },
  { code: "ja-JP", nativeName: "日本語", variantOf: null },
  { code: "ko-KR", nativeName: "한국어", variantOf: null },
];

/** A language code such as "de-DE"; any code the server declares. */
export type LocaleCode = string;

export const DEFAULT_LOCALE: LocaleCode = "en-US";

interface LanguageCode {
  code: string;
  variantOf?: string | null;
}

// Chinese written in traditional characters: Taiwan, Hong Kong, Macau, or an explicit script.
const TRADITIONAL_CHINESE = /^zh-(tw|hk|mo|hant)(-|$)/;

/**
 * The language of the list closest to a browser or address tag, the same way the server
 * matches it: exact code, then traditional Chinese, then the first language with the same
 * prefix that is not a regional variant (de-AT → de-DE, pt-PT → pt-BR, zh-SG → zh-CN).
 */
export function matchLocale(
  candidate: string | null | undefined,
  languages: readonly LanguageCode[] = LOCALES,
): LocaleCode | null {
  if (!candidate) {
    return null;
  }
  const normalized = candidate.trim().replace("_", "-").toLowerCase();
  const byCode = (code: string) =>
    languages.find((language) => language.code.toLowerCase() === code);
  const exact = byCode(normalized);
  if (exact) {
    return exact.code;
  }
  if (TRADITIONAL_CHINESE.test(normalized)) {
    const traditional = byCode("zh-tw");
    if (traditional) {
      return traditional.code;
    }
  }
  const prefix = `${normalized.split("-")[0] ?? ""}-`;
  const sameLanguage = languages.find(
    (language) => !language.variantOf && language.code.toLowerCase().startsWith(prefix),
  );
  return sameLanguage?.code ?? null;
}

/** The language a regional variant inherits its texts from, or null. */
export function parentOf(
  locale: LocaleCode,
  languages: readonly InterfaceLanguage[],
): LocaleCode | null {
  return languages.find((language) => language.code === locale)?.variantOf ?? null;
}
