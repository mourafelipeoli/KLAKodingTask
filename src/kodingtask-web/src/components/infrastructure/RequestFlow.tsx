import { useText } from "../../i18n/useText";
import type { InfrastructureReport } from "../../types/infrastructure";
import { formatNumber } from "../../utils/formatFacts";

interface RequestFlowProps {
  report: InfrastructureReport;
}

interface ConnectedService {
  name: string;
  role: string;
  icon: string;
  isActive: boolean;
}

// The path of one request, drawn with CSS: a column of steps on a phone, a row on a wide
// screen, joined by arrows. Below it, the services the container talks to, each marked
// active or not enabled on this instance.
export default function RequestFlow({ report }: RequestFlowProps) {
  const { t, locale } = useText();
  const { hosting, service, data, features, security, traffic } = report;

  const containerDetail = hosting.onAws
    ? [
        hosting.compute,
        hosting.vcpu !== null && hosting.memoryMiB !== null
          ? t("{{vcpu}} vCPU, {{memory}} MiB", {
              vcpu: formatNumber(hosting.vcpu, locale, 2),
              memory: formatNumber(hosting.memoryMiB, locale),
            })
          : null,
      ]
        .filter(Boolean)
        .join(" · ")
    : t("On this computer");

  const steps = [
    {
      icon: "bi-laptop",
      title: t("Your browser"),
      detail: t("Encrypted with HTTPS"),
      status: null,
    },
    {
      icon: "bi-signpost-split",
      title: t("Load balancer"),
      detail: t("Sends traffic only to healthy copies"),
      status:
        service?.healthyTargets != null && service.registeredTargets != null
          ? t("{{healthy}} of {{registered}} healthy", {
              healthy: formatNumber(service.healthyTargets, locale),
              registered: formatNumber(service.registeredTargets, locale),
            })
          : null,
    },
    {
      icon: "bi-box-seam",
      title: t("Container"),
      detail: containerDetail,
      status: service
        ? t("{{running}} of {{desired}} running", {
            running: formatNumber(service.running, locale),
            desired: formatNumber(service.desired, locale),
          })
        : null,
    },
    {
      icon: "bi-database",
      title: t("Database"),
      detail: [data.engine, data.managedBy].filter(Boolean).join(" · "),
      status: null,
    },
  ];

  const connected: ConnectedService[] = [
    {
      name: "Amazon Bedrock",
      role: t("AI conversion"),
      icon: "bi-stars",
      isActive: features.ai,
    },
    {
      name: "Parameter Store",
      role: t("Usage limits"),
      icon: "bi-sliders",
      isActive: security.limitsOutsideCode,
    },
    {
      name: "Secrets Manager",
      role: t("Keys and passwords"),
      icon: "bi-key",
      isActive: hosting.onAws,
    },
    {
      name: "CloudWatch",
      role: t("Logs and metrics"),
      icon: "bi-graph-up",
      isActive: traffic !== null,
    },
    {
      name: "Amazon SES",
      role: t("Email"),
      icon: "bi-envelope",
      isActive: features.email,
    },
    {
      name: "WhatsApp",
      role: t("Messages"),
      icon: "bi-whatsapp",
      isActive: features.whatsApp,
    },
    {
      name: "DeepL",
      role: t("Page translation"),
      icon: "bi-translate",
      isActive: features.machineTranslation,
    },
  ];

  return (
    <section className="request-flow" aria-labelledby="request-flow-title">
      <h2 id="request-flow-title" className="infrastructure-page__section-title">
        {t("How a request travels")}
      </h2>
      <ol className="request-flow__steps">
        {steps.map((step) => (
          <li key={step.icon} className="card request-flow__step">
            <i className={`bi ${step.icon} request-flow__icon`} aria-hidden="true" />
            <span className="request-flow__name">{step.title}</span>
            <span className="request-flow__detail">{step.detail}</span>
            {step.status && (
              <span className="badge badge--success request-flow__status">
                <i className="bi bi-check-circle" aria-hidden="true" />
                {step.status}
              </span>
            )}
          </li>
        ))}
      </ol>

      <h3 className="request-flow__connected-title">{t("Connected services")}</h3>
      <ul className="request-flow__services">
        {connected.map((item) => (
          <li
            key={item.name}
            className={`request-flow__service${item.isActive ? "" : " request-flow__service--off"}`}
          >
            <i
              className={`bi ${item.icon} request-flow__service-icon`}
              aria-hidden="true"
            />
            <span className="request-flow__service-text">
              <span className="request-flow__service-name">{item.name}</span>
              <span className="request-flow__service-role">{item.role}</span>
              <span
                className={`badge ${item.isActive ? "badge--success" : "badge--muted"} request-flow__service-state`}
              >
                <i
                  className={`bi ${item.isActive ? "bi-check-circle" : "bi-dash-circle"}`}
                  aria-hidden="true"
                />
                {item.isActive ? t("Active") : t("Not enabled here")}
              </span>
            </span>
          </li>
        ))}
      </ul>
    </section>
  );
}
