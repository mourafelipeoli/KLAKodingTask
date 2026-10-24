import { useEffect, useRef, useState } from "react";
import { useLocation, useSearchParams } from "react-router-dom";
import { useAppInfo } from "../../app/useAppInfo";
import { useAccountHandOver } from "../../hooks/useAccountHandOver";
import { useText } from "../../i18n/useText";
import { accountService } from "../../services/accountService";
import "./FmIdSignIn.css";

/** `?with=fmid`: the page was opened to sign in with fmID, so it starts by itself. */
const START_PARAM = "with";
const START_VALUE = "fmid";
/** `&retry=1`: the server's one automatic retry after a callback it could not match. */
const RETRY_PARAM = "retry";

interface FmIdSignInProps {
  /** Where to come back to after signing in: a path on this site. */
  returnUrl: string;
}

// "Continue with fmID", the only way to sign in: a form post that leaves for the provider
// and comes back signed in.
// A copy without fmID hands the visitor over to the same page on the deployed version,
// which starts the sign-in by itself.
export default function FmIdSignIn({ returnUrl }: FmIdSignInProps) {
  const { t, locale } = useText();
  const { features } = useAppInfo();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const handOverQuery = new URLSearchParams(location.search);
  handOverQuery.delete("error");
  handOverQuery.set(START_PARAM, START_VALUE);
  const handOver = useAccountHandOver(`${location.pathname}?${handOverQuery.toString()}`);
  const [isLeaving, setIsLeaving] = useState(false);
  const hasStarted = useRef(false);
  const shouldStart =
    features.externalLogin && searchParams.get(START_PARAM) === START_VALUE;
  const isRetry = searchParams.get(RETRY_PARAM) === "1";

  useEffect(() => {
    if (!shouldStart || hasStarted.current) {
      return;
    }
    hasStarted.current = true;
    // Coming back with the browser's Back button must not start it again.
    const current = new URL(window.location.href);
    current.searchParams.delete(START_PARAM);
    current.searchParams.delete(RETRY_PARAM);
    window.history.replaceState(window.history.state, "", current);
    void accountService.signIn(returnUrl, locale, isRetry);
  }, [shouldStart, isRetry, returnUrl, locale]);

  useEffect(() => {
    function handlePageShow(event: PageTransitionEvent) {
      if (event.persisted) {
        setIsLeaving(false);
      }
    }
    window.addEventListener("pageshow", handlePageShow);
    return () => window.removeEventListener("pageshow", handlePageShow);
  }, []);

  function handleClick() {
    if (handOver.isRequired) {
      handOver.open();
      return;
    }
    setIsLeaving(true);
    void accountService.signIn(returnUrl, locale);
  }

  const isBusy = isLeaving || shouldStart;

  return (
    <div className="fmid-sign-in">
      <button
        type="button"
        className="button button--primary fmid-sign-in__button"
        disabled={isBusy}
        onClick={handleClick}
      >
        {isBusy ? (
          <span className="spinner" aria-hidden="true" />
        ) : (
          <i className="bi bi-shield-lock" aria-hidden="true" />
        )}
        {t("Continue with fmID")}
      </button>
      <p className="fmid-sign-in__note">
        {t("fmID is the author's own OpenID Connect provider.")}{" "}
        {t(
          "No fmID account yet? Create one on the next page; you come back here signed in.",
        )}
      </p>
      {handOver.dialog}
    </div>
  );
}
