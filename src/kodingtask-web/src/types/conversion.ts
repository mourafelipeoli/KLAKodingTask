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
