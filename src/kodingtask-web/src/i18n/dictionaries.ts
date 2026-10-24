import type { LocaleCode } from "../config/locales";
import { DEFAULT_LOCALE } from "../config/locales";
import deDE from "./dictionaries/de-DE.json";
import enUS from "./dictionaries/en-US.json";
import esES from "./dictionaries/es-ES.json";
import frBE from "./dictionaries/fr-BE.json";
import frCA from "./dictionaries/fr-CA.json";
import frCH from "./dictionaries/fr-CH.json";
import frFR from "./dictionaries/fr-FR.json";
import itIT from "./dictionaries/it-IT.json";
import jaJP from "./dictionaries/ja-JP.json";
import koKR from "./dictionaries/ko-KR.json";
import plPL from "./dictionaries/pl-PL.json";
import ptBR from "./dictionaries/pt-BR.json";
import zhCN from "./dictionaries/zh-CN.json";
import zhTW from "./dictionaries/zh-TW.json";

export type Dictionary = Readonly<Record<string, string>>;

export type TextParameters = Readonly<Record<string, string | number>>;

// The dictionaries ship with the app, so the interface is fully translated even when the
// server cannot be reached. The server imports the same files as reviewed translations,
// and its dictionary (with machine translations and edits) takes precedence once loaded.
const BUNDLED: Readonly<Record<LocaleCode, Dictionary>> = {
  "en-US": enUS,
  "pt-BR": ptBR,
  "de-DE": deDE,
  "fr-FR": frFR,
  "fr-CA": frCA,
  "fr-CH": frCH,
  "fr-BE": frBE,
  "it-IT": itIT,
  "es-ES": esES,
  "pl-PL": plPL,
  "zh-CN": zhCN,
  "zh-TW": zhTW,
  "ja-JP": jaJP,
  "ko-KR": koKR,
};

function lookup(
  dictionary: Dictionary | null | undefined,
  key: string,
): string | undefined {
  return dictionary && Object.hasOwn(dictionary, key) ? dictionary[key] : undefined;
}

/** Whether the bundled English dictionary, the list of the code's own texts, has this key. */
export function isBundledText(key: string): boolean {
  return lookup(BUNDLED[DEFAULT_LOCALE], key) !== undefined;
}

/**
 * The server's dictionary first, then the bundled one, then the bundled one of the language
 * a regional variant inherits from, then the English key itself.
 */
export function translate(
  key: string,
  locale: LocaleCode,
  served: Dictionary | null,
  parent: LocaleCode | null,
  parameters?: TextParameters,
): string {
  const text =
    lookup(served, key) ??
    lookup(BUNDLED[locale], key) ??
    (parent ? lookup(BUNDLED[parent], key) : undefined) ??
    key;
  return parameters ? interpolate(text, parameters) : text;
}

function interpolate(text: string, parameters: TextParameters): string {
  return text.replace(/\{\{(\w+)\}\}/g, (placeholder, name: string) => {
    const value = parameters[name];
    return value === undefined ? placeholder : String(value);
  });
}
