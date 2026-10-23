import { useAppInfo } from "../../app/useAppInfo";
import { useDeployedVersion } from "../../hooks/useDeployedVersion";
import { useText } from "../../i18n/useText";
import { DeliveryChannel } from "../../types/conversion";
import { ConverterSection } from "../../utils/converterLink";

interface SendResultPanelProps {
  /** Called with the channel the visitor picked when this copy cannot deliver through it. */
  onUnavailable: (channel: DeliveryChannel) => void;
}

// On a copy without delivery credentials the buttons stay clickable and hand the visitor
// over to the deployed version, which lands on this panel with the same channel. On the
// deployed version itself a channel that is not switched on yet is disabled.
export default function SendResultPanel({ onUnavailable }: SendResultPanelProps) {
  const { t } = useText();
  const { features } = useAppInfo();
  const { isDeployedVersion } = useDeployedVersion();

  const channel = (name: DeliveryChannel, isAvailable: boolean) => ({
    "data-channel": name,
    disabled: !isAvailable && isDeployedVersion,
    onClick: isAvailable ? undefined : () => onUnavailable(name),
  });

  return (
    <section id={ConverterSection.Send} className="send-panel card">
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
          {...channel(DeliveryChannel.Email, features.email)}
        >
          <i className="bi bi-envelope" aria-hidden="true" />
          {t("By email")}
        </button>
        <button
          type="button"
          className="button button--secondary"
          {...channel(DeliveryChannel.WhatsApp, features.whatsApp)}
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
