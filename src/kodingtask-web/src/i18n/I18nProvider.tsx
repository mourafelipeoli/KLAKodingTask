import type { ReactNode } from "react";
import { useCallback, useEffect, useMemo, useState } from "react";
import type { LocaleCode } from "../config/locales";
import type { TextParameters } from "./dictionaries";
import { translate } from "./dictionaries";
import { I18nContext } from "./i18nContext";
import { resolveInitialLocale, storeLocale } from "./resolveLocale";

interface I18nProviderProps {
  children: ReactNode;
}

export function I18nProvider({ children }: I18nProviderProps) {
  const [locale, setLocaleState] = useState<LocaleCode>(resolveInitialLocale);

  useEffect(() => {
    document.documentElement.lang = locale;
  }, [locale]);

  const setLocale = useCallback((next: LocaleCode) => {
    setLocaleState(next);
    storeLocale(next);
  }, []);

  const value = useMemo(
    () => ({
      locale,
      setLocale,
      t: (key: string, parameters?: TextParameters) => translate(locale, key, parameters),
    }),
    [locale, setLocale],
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}
