import { useContext } from "react";
import type { AppInfo } from "../types/appInfo";
import { AppInfoContext } from "./appInfoContext";

export function useAppInfo(): AppInfo {
  return useContext(AppInfoContext);
}
