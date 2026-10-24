import { useEffect, useRef, useState, useTransition } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAccount } from "../../app/useAccount";
import { useAccountHandOver } from "../../hooks/useAccountHandOver";
import { useText } from "../../i18n/useText";
import { ROUTES } from "../../routes/routePaths";
import { conversionHistoryService } from "../../services/conversionHistoryService";
import { isAbortError } from "../../services/httpClient";
import type { ConversionHistoryEntry } from "../../types/conversion";
import { ConverterSection } from "../../utils/converterLink";
import { formatUsd } from "../../utils/formatAmount";
import { formatTimeAgo } from "../../utils/formatTimeAgo";
import { textKey } from "../../i18n/textKey";
import Notice from "../Notice";
import "./RecentConversions.css";

interface RecentConversionsProps {
  /** Changes after every conversion, so the list includes the one just made. */
  revision: number;
  nameOf: (languageCode: string) => string;
  onOpen: (entry: ConversionHistoryEntry) => void;
}

const LOAD_FAILED = textKey("The list could not be loaded right now.");
const CLEAR_FAILED = textKey("The list could not be cleared. Try again.");

// The signed-in visitor's last conversions, kept by the server for the account, so they
// follow the visitor to any device. Choosing one converts it again. Signed out, the card
// invites to sign in and comes back here afterwards.
export default function RecentConversions({
  revision,
  nameOf,
  onOpen,
}: RecentConversionsProps) {
  const { t, locale } = useText();
  const { account, isLoaded } = useAccount();
  const location = useLocation();
  const comeBack = `${location.pathname}${location.search}#${ConverterSection.Recent}`;
  const signInPath = `${ROUTES.signIn}?returnUrl=${encodeURIComponent(comeBack)}`;
  const handOver = useAccountHandOver(signInPath);
  const [entries, setEntries] = useState<readonly ConversionHistoryEntry[] | null>(null);
  const [loadedAt, setLoadedAt] = useState(0);
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [isClearing, startClearing] = useTransition();
  const sectionRef = useRef<HTMLElement>(null);
  const hasLanded = useRef(false);

  useEffect(() => {
    if (!account.isSignedIn) {
      return;
    }
    const controller = new AbortController();
    conversionHistoryService
      .list(controller.signal)
      .then((list) => {
        setEntries(list);
        setLoadedAt(Date.now());
        setErrorKey(null);
      })
      .catch((error: unknown) => {
        if (!isAbortError(error)) {
          setErrorKey(LOAD_FAILED);
        }
      });
    return () => controller.abort();
  }, [account.isSignedIn, revision]);

  // Coming back from signing in ("#recent"): the list is the reason for the trip.
  useEffect(() => {
    if (
      !hasLanded.current &&
      isLoaded &&
      window.location.hash === `#${ConverterSection.Recent}`
    ) {
      hasLanded.current = true;
      sectionRef.current?.scrollIntoView({ block: "start" });
    }
  }, [isLoaded, entries]);

  function handleClear() {
    startClearing(async () => {
      try {
        await conversionHistoryService.clear();
        setEntries([]);
        setErrorKey(null);
      } catch {
        setErrorKey(CLEAR_FAILED);
      }
    });
  }

  if (!isLoaded) {
    return null;
  }

  return (
    <section
      ref={sectionRef}
      id={ConverterSection.Recent}
      className="recent-conversions card"
    >
      <div className="recent-conversions__header">
        <h2 className="recent-conversions__title">{t("Your last conversions")}</h2>
        {account.isSignedIn && entries && entries.length > 0 && (
          <button
            type="button"
            className="button button--ghost button--small"
            disabled={isClearing}
            onClick={handleClear}
          >
            <i className="bi bi-trash3" aria-hidden="true" />
            {t("Clear the list")}
          </button>
        )}
      </div>

      {!account.isSignedIn && (
        <p className="recent-conversions__hint">
          {t("Sign in to keep your last conversions here, on any device.")}{" "}
          <Link
            to={signInPath}
            onClick={(event) => {
              if (handOver.isRequired) {
                event.preventDefault();
                handOver.open();
              }
            }}
          >
            {t("Sign in")}
          </Link>
        </p>
      )}

      {account.isSignedIn && entries && entries.length === 0 && (
        <p className="recent-conversions__hint">
          {t("Your conversions will appear here.")}
        </p>
      )}

      {account.isSignedIn && entries && entries.length > 0 && (
        <>
          <p className="recent-conversions__hint">
            {t("Choose one to convert it again.")}
          </p>
          <ol className="recent-conversions__list">
            {entries.map((entry) => (
              <li key={`${entry.amount}|${entry.language}`}>
                <button
                  type="button"
                  className="recent-conversions__item"
                  onClick={() => onOpen(entry)}
                >
                  <span className="recent-conversions__words" lang={entry.language}>
                    {entry.words}
                  </span>
                  <span className="recent-conversions__meta">
                    <span>{formatUsd(entry.dollars, entry.cents, locale)}</span>
                    <span>{nameOf(entry.language)}</span>
                    <time dateTime={entry.convertedAt}>
                      {formatTimeAgo(entry.convertedAt, loadedAt, locale)}
                    </time>
                  </span>
                </button>
              </li>
            ))}
          </ol>
        </>
      )}

      {errorKey && (
        <Notice tone="danger" role="alert">
          {t(errorKey)}
        </Notice>
      )}
      {handOver.dialog}
    </section>
  );
}
