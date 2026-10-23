import type { AppInfo } from "../types/appInfo";
import { httpClient } from "./httpClient";

export const appInfoService = {
  load: (signal?: AbortSignal) => httpClient.get<AppInfo>("/api/app-info", signal),
};
