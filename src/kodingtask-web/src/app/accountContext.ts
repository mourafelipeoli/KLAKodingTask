import { createContext } from "react";
import type { Account } from "../types/account";

export interface AccountValue {
  account: Account;
  /** False until the first answer from the server (or its failure). */
  isLoaded: boolean;
  /** Stores a new account state after a sign-in, sign-out or update, and renews the
   *  antiforgery token, which is bound to the session. */
  apply: (account: Account) => Promise<void>;
}

export const AccountContext = createContext<AccountValue | null>(null);
