import type { ConversionHistoryEntry } from "../types/conversion";
import { httpClient } from "./httpClient";

export const conversionHistoryService = {
  list: (signal?: AbortSignal) =>
    httpClient.get<ConversionHistoryEntry[]>("/api/account/conversions", signal),
  clear: () => httpClient.delete<undefined>("/api/account/conversions"),
};
