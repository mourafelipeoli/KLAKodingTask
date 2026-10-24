import { createContext } from "react";
import type { LocaleCode } from "../config/locales";
import type { InterfaceLanguage } from "../types/translation";
import type { TextParameters } from "./dictionaries";

export interface I18nValue {
  locale: LocaleCode;
  /** The interface languages the server declares (the bundled list until it answers). */
  languages: readonly InterfaceLanguage[];
  setLocale: (locale: LocaleCode) => void;
  t: (key: string, parameters?: TextParameters) => string;
}

export const I18nContext = createContext<I18nValue | null>(null);
