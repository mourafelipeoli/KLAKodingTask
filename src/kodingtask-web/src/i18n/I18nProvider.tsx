import type { ReactNode } from "react";
import { useCallback, useEffect, useMemo, useState } from "react";
import type { LocaleCode } from "../config/locales";
import { LOCALES, parentOf } from "../config/locales";
import { translationService } from "../services/translationService";
import type { InterfaceLanguage } from "../types/translation";
import type { Dictionary, TextParameters } from "./dictionaries";
import { isBundledText, translate } from "./dictionaries";
import { I18nContext } from "./i18nContext";
import { readLocaleCookie, resolveInitialLocale, storeLocale } from "./resolveLocale";
import { keepServedDictionary, readServedDictionary } from "./servedDictionaries";
import { reportUnknownText } from "./unknownTexts";

interface I18nProviderProps {
  children: ReactNode;
}

// The languages and their texts come from the server, so a language added to its
// InterfaceLanguage enum shows up here, translated, without a change to the client. The
// bundled list and dictionaries answer until the server does, and whenever it cannot.
export function I18nProvider({ children }: I18nProviderProps) {
  const [languages, setLanguages] = useState<readonly InterfaceLanguage[]>(LOCALES);
  const [isLanguageListFinal, setIsLanguageListFinal] = useState(false);
  // Null until the visitor picks a language in the menu; until then it is detected.
  const [chosenLocale, setChosenLocale] = useState<LocaleCode | null>(null);
  const detectedLocale = useMemo(() => resolveInitialLocale(languages), [languages]);
  const locale = chosenLocale ?? detectedLocale;

  const [served, setServed] = useState<Readonly<Record<LocaleCode, Dictionary>>>({});
  const kept = useMemo(() => readServedDictionary(locale), [locale]);
  const dictionary = served[locale] ?? kept;
  const parent = parentOf(locale, languages);

  useEffect(() => {
    const controller = new AbortController();
    translationService
      .languages(controller.signal)
      .then((list) => {
        if (list.length > 0) {
          setLanguages(list);
        }
      })
      .catch(() => {
        // The bundled list stays.
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsLanguageListFinal(true);
        }
      });
    return () => controller.abort();
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    translationService
      .dictionary(locale, controller.signal)
      .then((loaded) => {
        keepServedDictionary(loaded);
        setServed((current) => ({ ...current, [loaded.locale]: loaded.entries }));
      })
      .catch(() => {
        // The bundled dictionaries answer.
      });
    return () => controller.abort();
  }, [locale]);

  useEffect(() => {
    document.documentElement.lang = locale;
    // A detected language is shared with the server only once the list is final, so a
    // language the bundled list lacks never replaces the one the cookie remembers.
    if (isLanguageListFinal && chosenLocale === null && readLocaleCookie() !== locale) {
      storeLocale(locale, false);
    }
  }, [locale, chosenLocale, isLanguageListFinal]);

  const setLocale = useCallback((next: LocaleCode) => {
    setChosenLocale(next);
    storeLocale(next, true);
  }, []);

  const t = useCallback(
    (key: string, parameters?: TextParameters) => {
      if (!isBundledText(key) && !(dictionary && Object.hasOwn(dictionary, key))) {
        reportUnknownText(key);
      }
      return translate(key, locale, dictionary, parent, parameters);
    },
    [dictionary, locale, parent],
  );

  const value = useMemo(
    () => ({ locale, languages, setLocale, t }),
    [locale, languages, setLocale, t],
  );

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>;
}
