import { useEffect, useRef, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAccount } from "../app/useAccount";
import Notice from "../components/Notice";
import AccountCard from "../components/account/AccountCard";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { accountService } from "../services/accountService";
import { accountErrorKey, safeReturnUrl } from "../utils/accountErrors";
import "./AccountPages.css";

type Status =
  { kind: "confirming" } | { kind: "confirmed" } | { kind: "failed"; key: string };

// Opened from the confirmation email: confirms on arrival, then offers to continue where
// the visitor was (typically the result they wanted to send).
export default function ConfirmEmailPage() {
  const { t } = useText();
  useDocumentTitle(t("Email confirmation"));
  const { apply } = useAccount();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const returnUrl = safeReturnUrl(searchParams.get("returnUrl"));
  const [status, setStatus] = useState<Status>({ kind: "confirming" });
  const started = useRef(false);

  useEffect(() => {
    if (started.current) {
      return;
    }
    started.current = true;
    accountService
      .confirmEmail(searchParams.get("user") ?? "", searchParams.get("code") ?? "")
      .then(async (account) => {
        await apply(account);
        setStatus({ kind: "confirmed" });
      })
      .catch((error: unknown) =>
        setStatus({ kind: "failed", key: accountErrorKey(error) }),
      );
  }, [apply, searchParams]);

  return (
    <AccountCard title={t("Email confirmation")}>
      {status.kind === "confirming" && (
        <p className="account-card__note" role="status">
          <span className="spinner" aria-hidden="true" />{" "}
          {t("Confirming your email address…")}
        </p>
      )}
      {status.kind === "confirmed" && (
        <>
          <Notice tone="success">{t("Your email address is confirmed.")}</Notice>
          <button
            type="button"
            className="button button--primary"
            onClick={() => void navigate(returnUrl)}
          >
            {t("Continue")}
          </button>
        </>
      )}
      {status.kind === "failed" && (
        <Notice tone="danger" role="alert">
          {t(status.key)}
        </Notice>
      )}
    </AccountCard>
  );
}
