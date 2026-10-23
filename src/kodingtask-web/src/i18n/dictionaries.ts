import type { LocaleCode } from "../config/locales";
import { PARENT_LOCALE } from "../config/locales";
import deDE from "./dictionaries/de-DE.json";
import enUS from "./dictionaries/en-US.json";
import frBE from "./dictionaries/fr-BE.json";
import frCA from "./dictionaries/fr-CA.json";
import frCH from "./dictionaries/fr-CH.json";
import frFR from "./dictionaries/fr-FR.json";
import itIT from "./dictionaries/it-IT.json";
import jaJP from "./dictionaries/ja-JP.json";
import koKR from "./dictionaries/ko-KR.json";
import ptBR from "./dictionaries/pt-BR.json";
import zhCN from "./dictionaries/zh-CN.json";
import zhTW from "./dictionaries/zh-TW.json";

export type Dictionary = Readonly<Record<string, string>>;

export type TextParameters = Readonly<Record<string, string | number>>;

// The dictionaries ship with the app, so the interface is fully translated even when the
// server-side translation service is not configured (a fresh clone, for instance).
const BUNDLED: Record<LocaleCode, Dictionary> = {
  "en-US": enUS,
  "pt-BR": ptBR,
  "de-DE": deDE,
  "fr-FR": frFR,
  "fr-CA": frCA,
  "fr-CH": frCH,
  "fr-BE": frBE,
  "it-IT": itIT,
  "zh-CN": zhCN,
  "zh-TW": zhTW,
  "ja-JP": jaJP,
  "ko-KR": koKR,
};

export function translate(
  locale: LocaleCode,
  key: string,
  parameters?: TextParameters,
): string {
  const parent = PARENT_LOCALE[locale];
  const text = BUNDLED[locale][key] ?? (parent ? BUNDLED[parent][key] : undefined) ?? key;
  return parameters ? interpolate(text, parameters) : text;
}

function interpolate(text: string, parameters: TextParameters): string {
  return text.replace(/\{\{(\w+)\}\}/g, (placeholder, name: string) => {
    const value = parameters[name];
    return value === undefined ? placeholder : String(value);
  });
}
