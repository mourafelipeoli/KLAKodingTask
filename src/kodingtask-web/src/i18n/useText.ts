import { useContext } from "react";
import type { I18nValue } from "./i18nContext";
import { I18nContext } from "./i18nContext";

export function useText(): I18nValue {
  const value = useContext(I18nContext);
  if (!value) {
    throw new Error("useText must be used inside <I18nProvider>.");
  }
  return value;
}
