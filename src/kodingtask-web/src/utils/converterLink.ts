import { DeliveryChannel, Engine } from "../types/conversion";

// The parts of the converter page a link can land on; each is the id of its section.
export const ConverterSection = {
  Result: "result",
  Send: "send",
} as const;

export type ConverterSection = (typeof ConverterSection)[keyof typeof ConverterSection];

export interface ConverterLinkOptions {
  amount: string;
  language: string;
  engine: Engine;
  /** The interface language, so the other site opens in the same one. */
  uiLocale?: string;
  /** The section to scroll to once the amount is converted. */
  section?: ConverterSection;
  /** The delivery channel the visitor picked, to be completed on arrival. */
  channel?: DeliveryChannel;
}

export interface LinkTarget {
  section: ConverterSection | null;
  channel: DeliveryChannel | null;
}

// The converter page's address for an amount: "/?amount=45+100&language=de-DE&engine=ai".
// Opening it converts the amount again, which is what makes shared links and the hand-over
// to the deployed version show the same page. A hand-over also names where it came from
// ("#send", "send=email"), so the deployed page lands there and finishes the action.
export function converterPath({
  amount,
  language,
  engine,
  uiLocale,
  section,
  channel,
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
  if (channel) {
    query.set("send", channel);
  }
  return `/?${query.toString()}${section ? `#${section}` : ""}`;
}

const SECTIONS: readonly string[] = Object.values(ConverterSection);
const CHANNELS: readonly string[] = Object.values(DeliveryChannel);

export function readLinkTarget(query: URLSearchParams, hash: string): LinkTarget {
  const section = hash.replace(/^#/, "");
  const channel = query.get("send") ?? "";
  return {
    section: SECTIONS.includes(section) ? (section as ConverterSection) : null,
    channel: CHANNELS.includes(channel) ? (channel as DeliveryChannel) : null,
  };
}
