import type { ReactNode } from "react";
import { useEffect, useState } from "react";
import { appInfoService } from "../services/appInfoService";
import { isAbortError } from "../services/httpClient";
import type { AppInfo } from "../types/appInfo";
import { AppInfoContext, OFFLINE_APP_INFO } from "./appInfoContext";

interface AppInfoProviderProps {
  children: ReactNode;
}

export function AppInfoProvider({ children }: AppInfoProviderProps) {
  const [appInfo, setAppInfo] = useState<AppInfo>(OFFLINE_APP_INFO);

  useEffect(() => {
    const controller = new AbortController();
    appInfoService
      .load(controller.signal)
      .then(setAppInfo)
      .catch((error: unknown) => {
        if (!isAbortError(error)) {
          console.error(
            "Could not load /api/app-info; optional features stay off.",
            error,
          );
        }
      });
    return () => controller.abort();
  }, []);

  return <AppInfoContext.Provider value={appInfo}>{children}</AppInfoContext.Provider>;
}
