import type { ReactNode } from "react";
import { useCallback, useEffect, useMemo, useState } from "react";
import { accountService } from "../services/accountService";
import { isAbortError, setAntiforgeryToken } from "../services/httpClient";
import type { Account } from "../types/account";
import { SIGNED_OUT } from "../types/account";
import { AccountContext } from "./accountContext";

interface AccountProviderProps {
  children: ReactNode;
}

async function renewAntiforgeryToken(): Promise<void> {
  const { token } = await accountService.antiforgery();
  setAntiforgeryToken(token);
}

export function AccountProvider({ children }: AccountProviderProps) {
  const [account, setAccount] = useState<Account>(SIGNED_OUT);
  const [isLoaded, setIsLoaded] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    Promise.all([accountService.me(controller.signal), renewAntiforgeryToken()])
      .then(([current]) => {
        setAccount(current);
        setIsLoaded(true);
      })
      .catch((error: unknown) => {
        if (!isAbortError(error)) {
          console.error(
            "Could not load the account; treating the visitor as signed out.",
            error,
          );
          setIsLoaded(true);
        }
      });
    return () => controller.abort();
  }, []);

  const apply = useCallback(async (next: Account) => {
    setAccount(next);
    await renewAntiforgeryToken();
  }, []);

  const value = useMemo(() => ({ account, isLoaded, apply }), [account, isLoaded, apply]);
  return <AccountContext.Provider value={value}>{children}</AccountContext.Provider>;
}
