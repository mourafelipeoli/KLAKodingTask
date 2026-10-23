import { createContext } from "react";
import type { LocaleCode } from "../config/locales";
import type { TextParameters } from "./dictionaries";

export interface I18nValue {
  locale: LocaleCode;
  setLocale: (locale: LocaleCode) => void;
  t: (key: string, parameters?: TextParameters) => string;
}

export const I18nContext = createContext<I18nValue | null>(null);
