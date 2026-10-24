export interface ConversionResult {
  amount: string;
  normalizedAmount: string;
  dollars: number;
  cents: number;
  language: string;
  words: string;
  engine: string;
}

/** One of the signed-in visitor's last conversions, as the server keeps it. */
export interface ConversionHistoryEntry {
  /** The amount as the server normalised it, e.g. "45 100,00"; the converter accepts it. */
  amount: string;
  dollars: number;
  cents: number;
  language: string;
  words: string;
  /** ISO 8601 with offset. */
  convertedAt: string;
}

export interface OutputLanguage {
  code: string;
  englishName: string;
  nativeName: string;
}

export interface AiAnswer {
  words: string;
  agrees: boolean;
  model: string;
  latencyMs: number;
}

export interface AiConversionResult {
  deterministic: ConversionResult;
  ai: AiAnswer | null;
  aiError: string | null;
}

export const Engine = {
  Rules: "rules",
  Ai: "ai",
} as const;

export type Engine = (typeof Engine)[keyof typeof Engine];

export const DeliveryChannel = {
  Email: "email",
  WhatsApp: "whatsapp",
} as const;

export type DeliveryChannel = (typeof DeliveryChannel)[keyof typeof DeliveryChannel];
