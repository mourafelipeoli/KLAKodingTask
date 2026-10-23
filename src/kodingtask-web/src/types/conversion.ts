export interface ConversionResult {
  amount: string;
  normalizedAmount: string;
  dollars: number;
  cents: number;
  language: string;
  words: string;
  engine: string;
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
