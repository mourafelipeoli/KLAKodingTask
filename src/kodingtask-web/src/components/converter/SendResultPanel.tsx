import { useAppInfo } from "../../app/useAppInfo";
import { useText } from "../../i18n/useText";
import FeatureUnavailable from "../FeatureUnavailable";

export default function SendResultPanel() {
  const { t } = useText();
  const { features } = useAppInfo();

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
          disabled={!features.email}
        >
          <i className="bi bi-envelope" aria-hidden="true" />
          {t("By email")}
        </button>
        <button
          type="button"
          className="button button--secondary"
          disabled={!features.whatsApp}
        >
          <i className="bi bi-whatsapp" aria-hidden="true" />
          {t("By WhatsApp")}
        </button>
      </div>
      {!features.deliveries && <FeatureUnavailable />}
    </section>
  );
}
