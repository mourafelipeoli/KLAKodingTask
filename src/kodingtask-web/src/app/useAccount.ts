import { useContext } from "react";
import type { AccountValue } from "./accountContext";
import { AccountContext } from "./accountContext";

export function useAccount(): AccountValue {
  const value = useContext(AccountContext);
  if (!value) {
    throw new Error("useAccount must be used inside <AccountProvider>.");
  }
  return value;
}
