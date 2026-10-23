import { useState, useTransition } from "react";
import type { FormEvent } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAccount } from "../app/useAccount";
import Notice from "../components/Notice";
import AccountCard from "../components/account/AccountCard";
import AccountField from "../components/account/AccountField";
import SubmitButton from "../components/account/SubmitButton";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { ROUTES } from "../routes/routePaths";
import { accountService } from "../services/accountService";
import { accountErrorKey } from "../utils/accountErrors";
import "./AccountPages.css";

export default function ResetPasswordPage() {
  const { t } = useText();
  useDocumentTitle(t("Choose a new password"));
  const { apply } = useAccount();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [password, setPassword] = useState("");
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [isDone, setIsDone] = useState(false);
  const [isPending, startTransition] = useTransition();

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    startTransition(async () => {
      try {
        const account = await accountService.resetPassword(
          searchParams.get("user") ?? "",
          searchParams.get("code") ?? "",
          password,
        );
        await apply(account);
        setIsDone(true);
      } catch (error) {
        setErrorKey(accountErrorKey(error));
      }
    });
  }

  return (
    <AccountCard title={t("Choose a new password")}>
      {isDone ? (
        <>
          <Notice tone="success">
            {t("Your password has been changed and you are signed in.")}
          </Notice>
          <button
            type="button"
            className="button button--primary"
            onClick={() => void navigate(ROUTES.home)}
          >
            {t("Continue")}
          </button>
        </>
      ) : (
        <form className="account-form" noValidate onSubmit={handleSubmit}>
          <AccountField
            label={t("New password")}
            type="password"
            autoComplete="new-password"
            hint={t("At least 10 characters.")}
            value={password}
            onChange={setPassword}
          />
          {errorKey && (
            <Notice tone="danger" role="alert">
              {t(errorKey)}
            </Notice>
          )}
          <SubmitButton label={t("Choose a new password")} isPending={isPending} />
        </form>
      )}
    </AccountCard>
  );
}
