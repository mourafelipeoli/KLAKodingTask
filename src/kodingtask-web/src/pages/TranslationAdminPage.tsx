import { useEffect, useMemo, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { useAccount } from "../app/useAccount";
import Notice from "../components/Notice";
import TranslationRowEditor from "../components/translations/TranslationRowEditor";
import { DEFAULT_LOCALE } from "../config/locales";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { textKey } from "../i18n/textKey";
import { useText } from "../i18n/useText";
import { ROUTES } from "../routes/routePaths";
import { ApiError, isAbortError } from "../services/httpClient";
import { translationAdminService } from "../services/translationAdminService";
import type { AdminTranslationRow } from "../types/translation";
import "./TranslationAdminPage.css";

const FILTERS = {
  all: textKey("All texts"),
  unreviewed: textKey("Not reviewed"),
  missing: textKey("Missing"),
} as const;

type Filter = keyof typeof FILTERS;

const RELOAD_AFTER_RUN_MS = 8000;

interface LoadedRows {
  locale: string;
  rows: readonly AdminTranslationRow[];
}

const isMissing = (row: AdminTranslationRow) =>
  row.text === null && row.inherited === null;

const isUnreviewed = (row: AdminTranslationRow) => row.text !== null && !row.reviewed;

// The review screen of the translation system: every term (the interface's, the server's
// and the ones pages reported) in one language, with machine translations marked until
// someone reviews them. Only the addresses in the server's Admin:Emails list get here.
export default function TranslationAdminPage() {
  const { t, locale, languages } = useText();
  useDocumentTitle(t("Translations"));
  const { account, isLoaded } = useAccount();
  const location = useLocation();

  const targets = useMemo(
    () => languages.filter((language) => language.code !== DEFAULT_LOCALE),
    [languages],
  );
  const [chosenTarget, setChosenTarget] = useState<string | null>(null);
  const target =
    chosenTarget ?? (locale !== DEFAULT_LOCALE ? locale : (targets[0]?.code ?? locale));
  const [filter, setFilter] = useState<Filter>("all");
  const [search, setSearch] = useState("");
  const [loaded, setLoaded] = useState<LoadedRows | null>(null);
  const [revision, setRevision] = useState(0);
  const [failure, setFailure] = useState<unknown>(null);
  const [isRunStarted, setIsRunStarted] = useState(false);
  const rows = loaded?.locale === target ? loaded.rows : null;

  useEffect(() => {
    if (!account.isAdmin) {
      return;
    }
    const controller = new AbortController();
    translationAdminService
      .list(target, controller.signal)
      .then((list) => setLoaded({ locale: target, rows: list }))
      .catch((error: unknown) => {
        if (!isAbortError(error)) {
          setFailure(error);
        }
      });
    return () => controller.abort();
  }, [account.isAdmin, target, revision]);

  const visible = useMemo(() => {
    if (!rows) {
      return [];
    }
    const query = search.trim().toLowerCase();
    return rows.filter(
      (row) =>
        (filter === "all" ||
          (filter === "missing" && isMissing(row)) ||
          (filter === "unreviewed" && isUnreviewed(row))) &&
        (query === "" ||
          row.english.toLowerCase().includes(query) ||
          (row.text ?? "").toLowerCase().includes(query)),
    );
  }, [rows, filter, search]);

  // The server translates its own refusals into the visitor's language.
  const errorMessage =
    failure === null
      ? null
      : failure instanceof ApiError && failure.status > 0
        ? failure.message
        : t("The server is not reachable right now. Try again in a moment.");

  function handleRun() {
    setFailure(null);
    translationAdminService
      .run()
      .then(() => {
        setIsRunStarted(true);
        window.setTimeout(() => {
          setIsRunStarted(false);
          setRevision((current) => current + 1);
        }, RELOAD_AFTER_RUN_MS);
      })
      .catch((error: unknown) => setFailure(error));
  }

  function handleSaved(saved: AdminTranslationRow) {
    setLoaded((current) =>
      current && current.locale === target
        ? {
            ...current,
            rows: current.rows.map((row) => (row.termId === saved.termId ? saved : row)),
          }
        : current,
    );
  }

  if (!isLoaded) {
    return null;
  }

  if (!account.isAdmin) {
    const returnUrl = `${location.pathname}${location.search}`;
    return (
      <section className="container hero">
        <p className="eyebrow">{t("Administration")}</p>
        <h1 className="hero__title">{t("Translations")}</h1>
        <p className="hero__lead">
          {account.isSignedIn
            ? t("This page is for administrators.")
            : t("Sign in with an administrator account to review the translations.")}
        </p>
        {!account.isSignedIn && (
          <Link
            className="link-cta"
            to={`${ROUTES.signIn}?returnUrl=${encodeURIComponent(returnUrl)}`}
          >
            {t("Sign in")}
          </Link>
        )}
      </section>
    );
  }

  return (
    <div className="translation-admin">
      <section className="container hero">
        <p className="eyebrow">{t("Administration")}</p>
        <h1 className="hero__title">{t("Translations")}</h1>
        <p className="hero__lead">
          {t(
            "Every text of the interface, the emails and the error messages, in every language. Machine translations stay unreviewed until someone checks them.",
          )}
        </p>
      </section>

      <section className="container translation-admin__body">
        <div className="card translation-admin__toolbar">
          <label className="field">
            <span className="field__label">{t("Language")}</span>
            <select
              className="select"
              value={target}
              onChange={(event) => {
                setChosenTarget(event.target.value);
                setFailure(null);
              }}
            >
              {targets.map((language) => (
                <option key={language.code} value={language.code} lang={language.code}>
                  {language.nativeName}
                </option>
              ))}
            </select>
          </label>
          <label className="field">
            <span className="field__label">{t("Show")}</span>
            <select
              className="select"
              value={filter}
              onChange={(event) => setFilter(event.target.value as Filter)}
            >
              {Object.entries(FILTERS).map(([value, label]) => (
                <option key={value} value={value}>
                  {t(label)}
                </option>
              ))}
            </select>
          </label>
          <label className="field translation-admin__search">
            <span className="field__label">{t("Search")}</span>
            <input
              className="text-input"
              type="search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
          </label>
          <div className="translation-admin__actions">
            <button type="button" className="button button--primary" onClick={handleRun}>
              <i className="bi bi-translate" aria-hidden="true" />
              {t("Translate missing texts now")}
            </button>
            <a
              className="button button--ghost"
              href={translationAdminService.exportUrl(target)}
              download={`${target}.json`}
            >
              <i className="bi bi-download" aria-hidden="true" />
              {t("Download {{file}}", { file: `${target}.json` })}
            </a>
          </div>
        </div>

        {isRunStarted && (
          <Notice tone="info" role="status">
            {t("Machine translation started. The list reloads in a few seconds.")}
          </Notice>
        )}
        {errorMessage && (
          <Notice tone="danger" role="alert">
            {errorMessage}
          </Notice>
        )}

        {rows ? (
          <>
            <p className="translation-admin__summary" role="status">
              {t("{{count}} texts, {{missing}} missing, {{unreviewed}} not reviewed", {
                count: rows.length,
                missing: rows.filter(isMissing).length,
                unreviewed: rows.filter(isUnreviewed).length,
              })}
            </p>
            {visible.length > 0 ? (
              <ul className="translation-admin__list">
                {visible.map((row) => (
                  <TranslationRowEditor
                    key={`${revision}-${target}-${row.termId}`}
                    row={row}
                    locale={target}
                    onSaved={handleSaved}
                  />
                ))}
              </ul>
            ) : (
              <p className="translation-admin__empty">{t("Nothing matches.")}</p>
            )}
          </>
        ) : (
          !errorMessage && (
            <p className="translation-admin__loading" role="status">
              <span className="spinner" aria-hidden="true" />
              {t("Loading…")}
            </p>
          )
        )}
      </section>
    </div>
  );
}
