import { createContext } from "react";
import type { AppInfo } from "../types/appInfo";

export const DEFAULT_PUBLIC_SITE_URL = "https://klakodingtask.mourafelipe.com";

// Until /api/app-info answers (or if it never does), every optional feature counts as
// unavailable: the UI points to the hosted version instead of offering a broken button.
export const OFFLINE_APP_INFO: AppInfo = {
  version: null,
  commit: null,
  publicSiteUrl: DEFAULT_PUBLIC_SITE_URL,
  repositoryUrl: null,
  author: null,
  features: {
    email: false,
    whatsApp: false,
    ai: false,
    machineTranslation: false,
    externalLogin: false,
    deliveries: false,
  },
};

export const AppInfoContext = createContext<AppInfo>(OFFLINE_APP_INFO);
