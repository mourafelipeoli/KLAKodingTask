import type { ConversionResult, OutputLanguage } from "../types/conversion";
import { httpClient } from "./httpClient";

export const conversionService = {
  convert: (amount: string, language: string, signal?: AbortSignal) =>
    httpClient.post<ConversionResult>("/api/conversions", { amount, language }, signal),

  convertToAllLanguages: (amount: string, signal?: AbortSignal) =>
    httpClient.post<ConversionResult[]>(
      "/api/conversions/all-languages",
      { amount },
      signal,
    ),

  languages: (signal?: AbortSignal) =>
    httpClient.get<OutputLanguage[]>("/api/conversions/languages", signal),
};
