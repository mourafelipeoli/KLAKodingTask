import { useState, useTransition } from "react";
import type { FormEvent } from "react";
import { Link, useLocation, useNavigate, useSearchParams } from "react-router-dom";
import { useAccount } from "../app/useAccount";
import Notice from "../components/Notice";
import AccountCard from "../components/account/AccountCard";
import AccountField from "../components/account/AccountField";
import SubmitButton from "../components/account/SubmitButton";
import { useAccountHandOver } from "../hooks/useAccountHandOver";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { ROUTES } from "../routes/routePaths";
import { accountService } from "../services/accountService";
import { accountErrorKey, safeReturnUrl } from "../utils/accountErrors";
import "./AccountPages.css";

export default function RegisterPage() {
  const { t, locale } = useText();
  useDocumentTitle(t("Create an account"));
  const { apply } = useAccount();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const returnUrl = safeReturnUrl(searchParams.get("returnUrl"));
  const handOver = useAccountHandOver(`${location.pathname}${location.search}`);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [sentTo, setSentTo] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (handOver.isRequired) {
      handOver.open();
      return;
    }
    startTransition(async () => {
      try {
        const result = await accountService.register(email, password, locale, returnUrl);
        await apply(result.account);
        setErrorKey(result.confirmationSent ? null : accountErrorKey(null));
        setSentTo(result.account.email);
      } catch (error) {
        setErrorKey(accountErrorKey(error));
      }
    });
  }

  if (sentTo) {
    return (
      <AccountCard title={t("Create an account")}>
        {errorKey ? (
          <Notice tone="warning">{t(errorKey)}</Notice>
        ) : (
          <Notice tone="success">
            {t(
              "We sent a confirmation link to {{email}}. Open it to start sending results to yourself.",
              { email: sentTo },
            )}
          </Notice>
        )}
        <button
          type="button"
          className="button button--primary"
          onClick={() => void navigate(returnUrl)}
        >
          {t("Continue")}
        </button>
      </AccountCard>
    );
  }

  return (
    <AccountCard
      title={t("Create an account")}
      lead={t(
        "Accounts are only used to send conversion results to yourself by email or WhatsApp.",
      )}
    >
      <form className="account-form" noValidate onSubmit={handleSubmit}>
        <AccountField
          label={t("Email")}
          type="email"
          inputMode="email"
          autoComplete="email"
          value={email}
          onChange={setEmail}
        />
        <AccountField
          label={t("Password")}
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
        <SubmitButton label={t("Create an account")} isPending={isPending} />
      </form>
      <p className="account-card__links">
        <span>
          {t("Already have an account?")}{" "}
          <Link to={`${ROUTES.signIn}?returnUrl=${encodeURIComponent(returnUrl)}`}>
            {t("Sign in")}
          </Link>
        </span>
      </p>
      {handOver.dialog}
    </AccountCard>
  );
}
