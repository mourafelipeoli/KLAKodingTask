import { Link, useLocation, useNavigate } from "react-router-dom";
import { useAccount } from "../../app/useAccount";
import { useAccountHandOver } from "../../hooks/useAccountHandOver";
import { useText } from "../../i18n/useText";
import { ROUTES } from "../../routes/routePaths";
import { accountService } from "../../services/accountService";
import { SIGNED_OUT } from "../../types/account";
import "./AccountMenu.css";

const ACCOUNT_PAGES: readonly string[] = [
  ROUTES.signIn,
  ROUTES.register,
  ROUTES.confirmEmail,
  ROUTES.forgotPassword,
  ROUTES.resetPassword,
];

// Signed out: a "Sign in" link that comes back to the current page. Signed in: the
// address and a sign-out button.
export default function AccountMenu() {
  const { t } = useText();
  const { account, isLoaded, apply } = useAccount();
  const location = useLocation();
  const navigate = useNavigate();
  const signInPath = `${ROUTES.signIn}${location.search}`;
  const handOver = useAccountHandOver(signInPath);
  const returnUrl = ACCOUNT_PAGES.includes(location.pathname)
    ? ROUTES.home
    : `${location.pathname}${location.search}${location.hash}`;

  if (!isLoaded) {
    return null;
  }

  async function handleSignOut() {
    if (account.signedInWithFmId) {
      await accountService.signOutOfFmId(returnUrl);
      return;
    }
    await accountService.signOut();
    await apply(SIGNED_OUT);
    if (ACCOUNT_PAGES.includes(location.pathname)) {
      void navigate(ROUTES.home);
    }
  }

  if (account.isSignedIn) {
    return (
      <div className="account-menu">
        <span
          className="account-menu__identity"
          title={t("Signed in as {{email}}", { email: account.email ?? "" })}
        >
          <i className="bi bi-person-check" aria-hidden="true" />
          <span className="account-menu__email">{account.email}</span>
        </span>
        <button
          type="button"
          className="icon-button"
          aria-label={t("Sign out")}
          title={t("Sign out")}
          onClick={handleSignOut}
        >
          <i className="bi bi-box-arrow-right" aria-hidden="true" />
        </button>
      </div>
    );
  }

  return (
    <>
      <Link
        className="account-menu__sign-in"
        to={`${ROUTES.signIn}?returnUrl=${encodeURIComponent(returnUrl)}`}
        aria-label={t("Sign in")}
        title={t("Sign in")}
        onClick={(event) => {
          if (handOver.isRequired) {
            event.preventDefault();
            handOver.open();
          }
        }}
      >
        <i className="bi bi-person" aria-hidden="true" />
        <span className="account-menu__label">{t("Sign in")}</span>
      </Link>
      {handOver.dialog}
    </>
  );
}
