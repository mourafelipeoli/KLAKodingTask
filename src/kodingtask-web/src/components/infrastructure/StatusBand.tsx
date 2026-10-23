import { useText } from "../../i18n/useText";
import type { InfrastructureReport } from "../../types/infrastructure";
import { formatNumber, formatRelative } from "../../utils/formatFacts";

interface StatusBandProps {
  report: InfrastructureReport;
}

// The headline figures in a dark band, like the key figures on the converter page. A
// figure this instance could not read is left out rather than shown as unknown.
export default function StatusBand({ report }: StatusBandProps) {
  const { t, locale } = useText();
  const { hosting, service, traffic } = report;
  const checkedAt = new Date(report.checkedAt);

  return (
    <section className="status-band" aria-label={t("Live status")}>
      <div className="container status-band__inner">
        <div className="status-band__item">
          <span className="status-band__value">
            <span className="status-band__dot" aria-hidden="true" />
            {t("Online")}
          </span>
          <span className="status-band__label">{t("Status")}</span>
        </div>
        {service && (
          <div className="status-band__item">
            <span className="status-band__value">
              {t("{{running}} of {{desired}}", {
                running: formatNumber(service.running, locale),
                desired: formatNumber(service.desired, locale),
              })}
            </span>
            <span className="status-band__label">{t("Copies running")}</span>
          </div>
        )}
        {hosting.startedAt && (
          <div className="status-band__item">
            <span className="status-band__value">
              {formatRelative(hosting.startedAt, locale, checkedAt)}
            </span>
            <span className="status-band__label">{t("Started")}</span>
          </div>
        )}
        {traffic?.requests24h != null && (
          <div className="status-band__item">
            <span className="status-band__value">
              {formatNumber(traffic.requests24h, locale)}
            </span>
            <span className="status-band__label">{t("Requests, last 24 hours")}</span>
          </div>
        )}
        {traffic?.averageResponseMs24h != null && (
          <div className="status-band__item">
            <span className="status-band__value">
              {t("{{value}} ms", {
                value: formatNumber(
                  traffic.averageResponseMs24h,
                  locale,
                  traffic.averageResponseMs24h < 10 ? 1 : 0,
                ),
              })}
            </span>
            <span className="status-band__label">{t("Average response time")}</span>
          </div>
        )}
      </div>
    </section>
  );
}
