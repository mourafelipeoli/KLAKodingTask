import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import Notice from "../components/Notice";
import InfrastructureDetails from "../components/infrastructure/InfrastructureDetails";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { ROUTES } from "../routes/routePaths";
import { infrastructureService } from "../services/infrastructureService";
import { isAbortError } from "../services/httpClient";
import type { InfrastructureReport } from "../types/infrastructure";
import "./InfrastructurePage.css";

export default function InfrastructurePage() {
  const { t } = useText();
  useDocumentTitle(t("Where this runs"));
  const [report, setReport] = useState<InfrastructureReport | null>(null);
  const [hasFailed, setHasFailed] = useState(false);

  useEffect(() => {
    const controller = new AbortController();
    infrastructureService
      .load(controller.signal)
      .then(setReport)
      .catch((error: unknown) => {
        if (!isAbortError(error)) {
          console.error("Could not load /api/infrastructure.", error);
          setHasFailed(true);
        }
      });
    return () => controller.abort();
  }, []);

  return (
    <div className="infrastructure-page">
      <section className="container hero">
        <p className="eyebrow">{t("Infrastructure")}</p>
        <h1 className="hero__title">{t("The infrastructure behind this page")}</h1>
        <p className="hero__lead">
          {t(
            "A live look at how this app is hosted, deployed and protected. The figures come from AWS when you open the page, and nothing here identifies an account, a resource or an address.",
          )}
        </p>
      </section>

      {report ? (
        <InfrastructureDetails report={report} />
      ) : (
        <div className="container infrastructure-page__state">
          {hasFailed ? (
            <Notice tone="danger" role="alert">
              {t("Could not load the infrastructure facts. Try again in a moment.")}
            </Notice>
          ) : (
            <p className="infrastructure-page__loading" role="status">
              <span className="spinner" aria-hidden="true" />
              {t("Loading the infrastructure facts…")}
            </p>
          )}
        </div>
      )}

      <div className="container infrastructure-page__back">
        <Link className="link-cta" to={ROUTES.home}>
          {t("Back to the converter")}
        </Link>
      </div>
    </div>
  );
}
