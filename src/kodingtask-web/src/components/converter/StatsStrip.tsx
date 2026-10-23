import { useText } from "../../i18n/useText";

export default function StatsStrip() {
  const { t } = useText();

  return (
    <section className="stats-strip" aria-label={t("Key figures")}>
      <div className="container stats-strip__inner">
        <div className="stats-strip__item">
          <span className="stats-strip__value">8</span>
          <span className="stats-strip__label">{t("output languages")}</span>
        </div>
        <div className="stats-strip__item">
          <span className="stats-strip__value">4</span>
          <span className="stats-strip__label">{t("variants of French")}</span>
        </div>
        <div className="stats-strip__item">
          <span className="stats-strip__value">999 999 999,99</span>
          <span className="stats-strip__label">{t("maximum amount")}</span>
        </div>
      </div>
    </section>
  );
}
