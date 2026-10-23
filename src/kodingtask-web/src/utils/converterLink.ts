import { Engine } from "../types/conversion";

export interface ConverterLinkOptions {
  amount: string;
  language: string;
  engine: Engine;
  /** The interface language, so the other site opens in the same one. */
  uiLocale?: string;
}

// The converter page's address for an amount: "/?amount=45+100&language=de-DE&engine=ai".
// Opening it converts the amount again, which is what makes shared links and the hand-over
// to the deployed version show the same result.
export function converterPath({
  amount,
  language,
  engine,
  uiLocale,
}: ConverterLinkOptions) {
  const query = new URLSearchParams();
  if (amount.trim()) {
    query.set("amount", amount.trim());
  }
  query.set("language", language);
  if (engine === Engine.Ai) {
    query.set("engine", Engine.Ai);
  }
  if (uiLocale) {
    query.set("lang", uiLocale);
  }
  return `/?${query.toString()}`;
}
