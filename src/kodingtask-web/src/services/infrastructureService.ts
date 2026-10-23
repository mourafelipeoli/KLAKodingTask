import type { InfrastructureReport } from "../types/infrastructure";
import { httpClient } from "./httpClient";

export const infrastructureService = {
  load: (signal?: AbortSignal) =>
    httpClient.get<InfrastructureReport>("/api/infrastructure", signal),
};
