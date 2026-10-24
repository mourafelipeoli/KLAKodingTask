import { useSearchParams } from "react-router-dom";
import Notice from "../components/Notice";
import AccountCard from "../components/account/AccountCard";
import FmIdSignIn from "../components/account/FmIdSignIn";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { fmIdErrorKey, safeReturnUrl } from "../utils/accountErrors";
import "./AccountPages.css";

// fmID is the only way in. Creating an fmID account, confirming the address and resetting
// the password all happen on the provider's pages, which come back here signed in.
export default function SignInPage() {
  const { t } = useText();
  useDocumentTitle(t("Sign in"));
  const [searchParams] = useSearchParams();
  const returnUrl = safeReturnUrl(searchParams.get("returnUrl"));
  const errorKey = fmIdErrorKey(searchParams.get("error"));

  return (
    <AccountCard
      title={t("Sign in")}
      lead={t(
        "Accounts are only used to send conversion results to yourself by email or WhatsApp.",
      )}
    >
      {errorKey && (
        <Notice tone="danger" role="alert">
          {t(errorKey)}
        </Notice>
      )}
      <FmIdSignIn returnUrl={returnUrl} />
    </AccountCard>
  );
}
