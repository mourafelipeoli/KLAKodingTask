import { useAppInfo } from "../../app/useAppInfo";
import RichText from "../../i18n/RichText";
import { textKey } from "../../i18n/textKey";
import { useText } from "../../i18n/useText";
import { ROUTES } from "../../routes/routePaths";
import type { InfrastructureReport } from "../../types/infrastructure";
import {
  formatDate,
  formatDateTime,
  formatNumber,
  formatPercent,
  formatSpend,
} from "../../utils/formatFacts";
import { hostOf } from "../../utils/hostOf";
import Notice from "../Notice";
import FactCard from "./FactCard";
import FactList from "./FactList";
import RequestFlow from "./RequestFlow";
import RequestSparkline from "./RequestSparkline";
import StatusBand from "./StatusBand";
import UsageMeter from "./UsageMeter";

const ROLLOUT_STATES: Record<string, string> = {
  COMPLETED: textKey("Completed"),
  IN_PROGRESS: textKey("In progress"),
  FAILED: textKey("Failed"),
};

interface InfrastructureDetailsProps {
  report: InfrastructureReport;
}

export default function InfrastructureDetails({ report }: InfrastructureDetailsProps) {
  const { t, locale } = useText();
  const { publicSiteUrl } = useAppInfo();
  const { hosting, service, traffic, build, data, security, guardrails } = report;
  const onOff = (value: boolean) => (value ? t("On") : t("Off"));
  const yesNo = (value: boolean) => (value ? t("Yes") : t("No"));
  const milliseconds = (value: number) =>
    t("{{value}} ms", { value: formatNumber(value, locale, value < 10 ? 1 : 0) });

  return (
    <>
      {!hosting.onAws && (
        <div className="container infrastructure-page__state">
          <Notice tone="info">
            <RichText
              text={t(
                "You are running this app on your own computer, so there is no cloud to describe. To see the live infrastructure, open {{link}}.",
              )}
              slots={{
                link: (
                  <a
                    href={`${publicSiteUrl}${ROUTES.infrastructure}`}
                    target="_blank"
                    rel="noopener"
                  >
                    {hostOf(publicSiteUrl)}
                  </a>
                ),
              }}
            />
          </Notice>
        </div>
      )}

      <StatusBand report={report} />

      <div className="container infrastructure-page__body">
        <RequestFlow report={report} />

        <section aria-labelledby="facts-title">
          <h2 id="facts-title" className="infrastructure-page__section-title">
            {t("The details")}
          </h2>
          <div className="infrastructure-page__grid">
            <FactCard icon="bi-cloud" title={t("Hosting")}>
              <FactList
                facts={[
                  { label: t("Region"), value: hosting.regionName },
                  { label: t("Availability zone"), value: hosting.availabilityZone },
                  { label: t("Compute"), value: hosting.compute },
                  {
                    label: t("Size"),
                    value:
                      hosting.vcpu !== null && hosting.memoryMiB !== null
                        ? t("{{vcpu}} vCPU, {{memory}} MiB", {
                            vcpu: formatNumber(hosting.vcpu, locale, 2),
                            memory: formatNumber(hosting.memoryMiB, locale),
                          })
                        : null,
                  },
                  { label: t("Runtime"), value: hosting.runtime },
                  {
                    label: t("Operating system"),
                    value: `${hosting.operatingSystem} (${hosting.architecture})`,
                  },
                  {
                    label: t("Started"),
                    value: hosting.startedAt
                      ? formatDateTime(hosting.startedAt, locale)
                      : null,
                  },
                ]}
              />
            </FactCard>

            <FactCard icon="bi-rocket-takeoff" title={t("Deployment")}>
              <FactList
                facts={[
                  { label: t("Version"), value: build.version },
                  {
                    label: t("Commit"),
                    value: build.commit ? (
                      build.commitUrl ? (
                        <a href={build.commitUrl} target="_blank" rel="noopener">
                          <code>{build.commit}</code>
                        </a>
                      ) : (
                        <code>{build.commit}</code>
                      )
                    ) : null,
                  },
                  {
                    label: t("Built"),
                    value: build.builtAt ? formatDateTime(build.builtAt, locale) : null,
                  },
                  {
                    label: t("Last deployed"),
                    value: service?.lastDeployedAt
                      ? formatDateTime(service.lastDeployedAt, locale)
                      : null,
                  },
                  {
                    label: t("Rollout"),
                    value: service?.rolloutState
                      ? t(ROLLOUT_STATES[service.rolloutState] ?? service.rolloutState)
                      : null,
                  },
                  {
                    label: t("Automatic rollback"),
                    value: service ? onOff(service.automaticRollback) : null,
                  },
                ]}
              />
            </FactCard>

            <FactCard icon="bi-activity" title={t("Traffic")}>
              {traffic?.requestsPerHour ? (
                <RequestSparkline
                  perHour={traffic.requestsPerHour}
                  checkedAt={report.checkedAt}
                />
              ) : null}
              <FactList
                facts={[
                  {
                    label: t("Requests, last 24 hours"),
                    value:
                      traffic?.requests24h != null
                        ? formatNumber(traffic.requests24h, locale)
                        : null,
                  },
                  {
                    label: t("Average response time"),
                    value:
                      traffic?.averageResponseMs24h != null
                        ? milliseconds(traffic.averageResponseMs24h)
                        : null,
                  },
                ]}
              />
              {traffic?.cpuPercent1h != null && (
                <UsageMeter
                  kind="load"
                  label={t("CPU, last hour")}
                  valueText={formatPercent(traffic.cpuPercent1h, locale)}
                  ratio={traffic.cpuPercent1h / 100}
                />
              )}
              {traffic?.memoryPercent1h != null && (
                <UsageMeter
                  kind="load"
                  label={t("Memory, last hour")}
                  valueText={formatPercent(traffic.memoryPercent1h, locale)}
                  ratio={traffic.memoryPercent1h / 100}
                />
              )}
            </FactCard>

            <FactCard icon="bi-database" title={t("Data")}>
              <FactList
                facts={[
                  { label: t("Engine"), value: data.engine },
                  { label: t("Version"), value: data.version },
                  { label: t("Managed by"), value: data.managedBy },
                  {
                    label: t("Storage"),
                    value: data.persistent
                      ? t("Kept across deploys")
                      : t("Inside the container, emptied at every deploy"),
                  },
                  {
                    label: t("Standby in a second zone"),
                    value: data.multiAz === null ? null : yesNo(data.multiAz),
                  },
                ]}
              />
            </FactCard>

            <FactCard icon="bi-shield-lock" title={t("Security")}>
              <FactList
                facts={[
                  { label: "HTTPS", value: onOff(security.https) },
                  {
                    label: t("Certificate valid until"),
                    value: security.certificateValidUntil
                      ? formatDate(security.certificateValidUntil, locale)
                      : null,
                  },
                  {
                    label: t("Public IP address"),
                    value: service
                      ? service.publicIpAddress
                        ? t("Assigned")
                        : t("None, only the load balancer reaches the container")
                      : null,
                  },
                  {
                    label: t("Secrets"),
                    value: hosting.onAws
                      ? t("AWS Secrets Manager, injected at start")
                      : null,
                  },
                  {
                    label: t("Usage limits"),
                    value: security.limitsOutsideCode
                      ? t("Outside the code, in AWS Systems Manager Parameter Store")
                      : t("In the configuration file"),
                  },
                ]}
              />
            </FactCard>

            <FactCard icon="bi-speedometer2" title={t("Guardrails")}>
              <FactList
                facts={[
                  {
                    label: t("Paid features"),
                    value: guardrails.killSwitch ? (
                      <span className="badge badge--warning">
                        <i className="bi bi-pause-circle" aria-hidden="true" />
                        {t("Paused")}
                      </span>
                    ) : (
                      <span className="badge badge--success">
                        <i className="bi bi-check-circle" aria-hidden="true" />
                        {t("Available")}
                      </span>
                    ),
                  },
                ]}
              />
              <UsageMeter
                label={t("AI calls today")}
                valueText={t("{{used}} of {{limit}}", {
                  used: formatNumber(guardrails.aiCallsToday, locale),
                  limit: formatNumber(guardrails.aiCallsPerDay, locale),
                })}
                ratio={
                  guardrails.aiCallsPerDay > 0
                    ? guardrails.aiCallsToday / guardrails.aiCallsPerDay
                    : 1
                }
              />
              <UsageMeter
                label={t("AI spending this month")}
                valueText={t("{{used}} of {{limit}}", {
                  used: formatSpend(guardrails.aiSpendThisMonthUsd, locale),
                  limit: formatSpend(guardrails.aiMonthlyBudgetUsd, locale),
                })}
                ratio={
                  guardrails.aiMonthlyBudgetUsd > 0
                    ? guardrails.aiSpendThisMonthUsd / guardrails.aiMonthlyBudgetUsd
                    : 1
                }
              />
              <p className="fact-card__note">
                {t(
                  "The code can read these limits but not change them. When one is reached, the paid feature refuses politely until the next day or month.",
                )}
              </p>
            </FactCard>
          </div>
        </section>

        <p className="infrastructure-page__checked">
          {t("Checked at {{time}}. The AWS figures refresh every five minutes.", {
            time: formatDateTime(report.checkedAt, locale),
          })}
        </p>
      </div>
    </>
  );
}
