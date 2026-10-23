import { useAppInfo } from "../../app/useAppInfo";
import { useDeployedVersion } from "../../hooks/useDeployedVersion";
import { useText } from "../../i18n/useText";

interface SendResultPanelProps {
  /** Called when the visitor picks a channel this copy cannot deliver through. */
  onUnavailable: () => void;
}

// On a copy without delivery credentials the buttons stay clickable and offer the deployed
// version; on the deployed version itself a channel that is not switched on yet is disabled.
export default function SendResultPanel({ onUnavailable }: SendResultPanelProps) {
  const { t } = useText();
  const { features } = useAppInfo();
  const { isDeployedVersion } = useDeployedVersion();

  const channel = (isAvailable: boolean) => ({
    disabled: !isAvailable && isDeployedVersion,
    onClick: isAvailable ? undefined : onUnavailable,
  });

  return (
    <section className="send-panel card">
      <div className="send-panel__header">
        <h2 className="send-panel__title">{t("Send this result")}</h2>
        <p className="send-panel__lead">
          {t("Receive the words by email or WhatsApp after signing in.")}
        </p>
      </div>
      <div className="send-panel__channels">
        <button
          type="button"
          className="button button--secondary"
          {...channel(features.email)}
        >
          <i className="bi bi-envelope" aria-hidden="true" />
          {t("By email")}
        </button>
        <button
          type="button"
          className="button button--secondary"
          {...channel(features.whatsApp)}
        >
          <i className="bi bi-whatsapp" aria-hidden="true" />
          {t("By WhatsApp")}
        </button>
      </div>
      {isDeployedVersion && !features.deliveries && (
        <p className="send-panel__soon">
          {t("Sending by email and WhatsApp is coming soon.")}
        </p>
      )}
    </section>
  );
}
