import type { DeliveryChannel } from "../types/conversion";
import type { DeliveryResult } from "../types/account";
import { httpClient } from "./httpClient";

export const deliveryService = {
  send: (channel: DeliveryChannel, amount: string, language: string, uiLocale: string) =>
    httpClient.post<DeliveryResult>("/api/deliveries", {
      channel,
      amount,
      language,
      uiLocale,
    }),
};
