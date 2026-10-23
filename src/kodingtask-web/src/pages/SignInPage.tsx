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

export default function SignInPage() {
  const { t } = useText();
  useDocumentTitle(t("Sign in"));
  const { apply } = useAccount();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const returnUrl = safeReturnUrl(searchParams.get("returnUrl"));
  const handOver = useAccountHandOver(`${location.pathname}${location.search}`);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    if (handOver.isRequired) {
      handOver.open();
      return;
    }
    startTransition(async () => {
      try {
        await apply(await accountService.signIn(email, password));
        void navigate(returnUrl);
      } catch (error) {
        setErrorKey(accountErrorKey(error));
      }
    });
  }

  return (
    <AccountCard
      title={t("Sign in")}
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
          autoComplete="current-password"
          value={password}
          onChange={setPassword}
        />
        {errorKey && (
          <Notice tone="danger" role="alert">
            {t(errorKey)}
          </Notice>
        )}
        <SubmitButton label={t("Sign in")} isPending={isPending} />
      </form>
      <p className="account-card__links">
        <Link to={ROUTES.forgotPassword}>{t("Forgot your password?")}</Link>
        <span>
          {t("No account yet?")}{" "}
          <Link to={`${ROUTES.register}?returnUrl=${encodeURIComponent(returnUrl)}`}>
            {t("Create an account")}
          </Link>
        </span>
      </p>
      {handOver.dialog}
    </AccountCard>
  );
}
