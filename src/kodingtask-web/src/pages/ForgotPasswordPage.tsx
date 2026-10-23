import { useState, useTransition } from "react";
import type { FormEvent } from "react";
import { useLocation } from "react-router-dom";
import Notice from "../components/Notice";
import AccountCard from "../components/account/AccountCard";
import AccountField from "../components/account/AccountField";
import SubmitButton from "../components/account/SubmitButton";
import { useAccountHandOver } from "../hooks/useAccountHandOver";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { accountService } from "../services/accountService";
import { accountErrorKey } from "../utils/accountErrors";
import "./AccountPages.css";

export default function ForgotPasswordPage() {
  const { t, locale } = useText();
  useDocumentTitle(t("Reset your password"));
  const location = useLocation();
  const handOver = useAccountHandOver(`${location.pathname}${location.search}`);
  const [email, setEmail] = useState("");
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [isSent, setIsSent] = useState(false);
  const [isPending, startTransition] = useTransition();

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (handOver.isRequired) {
      handOver.open();
      return;
    }
    startTransition(async () => {
      try {
        await accountService.forgotPassword(email, locale);
        setIsSent(true);
      } catch (error) {
        setErrorKey(accountErrorKey(error));
      }
    });
  }

  return (
    <AccountCard
      title={t("Reset your password")}
      lead={t(
        "Type the email of your account and we will send you a link to choose a new password.",
      )}
    >
      {isSent ? (
        <Notice tone="success">
          {t(
            "If an account exists for this address, a link to reset the password is on its way.",
          )}
        </Notice>
      ) : (
        <form className="account-form" noValidate onSubmit={handleSubmit}>
          <AccountField
            label={t("Email")}
            type="email"
            inputMode="email"
            autoComplete="email"
            value={email}
            onChange={setEmail}
          />
          {errorKey && (
            <Notice tone="danger" role="alert">
              {t(errorKey)}
            </Notice>
          )}
          <SubmitButton label={t("Send the link")} isPending={isPending} />
        </form>
      )}
      {handOver.dialog}
    </AccountCard>
  );
}
