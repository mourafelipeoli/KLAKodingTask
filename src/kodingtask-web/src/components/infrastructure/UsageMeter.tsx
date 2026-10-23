import { useText } from "../../i18n/useText";

interface UsageMeterProps {
  label: string;
  /** The value as people read it, e.g. "12 of 200" or "$0.42 of $2.00". */
  valueText: string;
  /** Used share of the limit, 0 to 1 (or more when a limit was lowered after use). */
  ratio: number;
  /** A limit warns when it is close and when it is reached; a load (CPU) never does. */
  kind?: "limit" | "load";
}

const WARNING_RATIO = 0.8;

export default function UsageMeter({
  label,
  valueText,
  ratio,
  kind = "limit",
}: UsageMeterProps) {
  const { t } = useText();
  const percent = Math.max(0, Math.min(100, ratio * 100));
  const level =
    kind === "load"
      ? "normal"
      : ratio >= 1
        ? "danger"
        : ratio >= WARNING_RATIO
          ? "warning"
          : "normal";

  return (
    <div className="usage-meter">
      <div className="usage-meter__header">
        <span className="usage-meter__label">{label}</span>
        <span className="usage-meter__value">{valueText}</span>
      </div>
      <div
        className="usage-meter__track"
        role="meter"
        aria-label={label}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={Math.round(percent)}
        aria-valuetext={valueText}
      >
        <span
          className={`usage-meter__fill usage-meter__fill--${level}`}
          style={{ width: `${percent}%` }}
        />
      </div>
      {level === "danger" && (
        <p className="usage-meter__status usage-meter__status--danger">
          <i className="bi bi-x-octagon" aria-hidden="true" />
          {t("Limit reached")}
        </p>
      )}
      {level === "warning" && (
        <p className="usage-meter__status usage-meter__status--warning">
          <i className="bi bi-exclamation-triangle" aria-hidden="true" />
          {t("Close to the limit")}
        </p>
      )}
    </div>
  );
}
