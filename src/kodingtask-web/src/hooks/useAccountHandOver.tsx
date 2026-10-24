import type { ReactNode } from "react";
import { useState } from "react";
import { useAppInfo } from "../app/useAppInfo";
import DeployedVersionDialog from "../components/DeployedVersionDialog";
import { useDeployedVersion } from "./useDeployedVersion";

export interface AccountHandOver {
  /** True on a copy without the feature: actions continue on the deployed version. */
  isRequired: boolean;
  open: () => void;
  dialog: ReactNode;
}

// Accounts with a password need email (to confirm the address); fmID needs its own client
// settings (pass isAvailable). A copy without them keeps every account action usable and
// hands the visitor over to the same page on the deployed version.
export function useAccountHandOver(
  pathAndQuery: string,
  isAvailable?: boolean,
): AccountHandOver {
  const { features } = useAppInfo();
  const { isDeployedVersion, urlFor } = useDeployedVersion();
  const [isOpen, setIsOpen] = useState(false);

  return {
    isRequired: !(isAvailable ?? features.accounts) && !isDeployedVersion,
    open: () => setIsOpen(true),
    dialog: (
      <DeployedVersionDialog
        isOpen={isOpen}
        url={urlFor(pathAndQuery)}
        onClose={() => setIsOpen(false)}
      />
    ),
  };
}
