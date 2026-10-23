import { useText } from "../../i18n/useText";
import type { AiAnswer } from "../../types/conversion";
import { AI_UNAVAILABLE, aiErrorKey } from "../../utils/aiErrors";
import Notice from "../Notice";

interface AiAnswerPanelProps {
  answer: AiAnswer | null;
  errorCode: string | null;
  language: string;
  isLoading: boolean;
}

export default function AiAnswerPanel({
  answer,
  errorCode,
  language,
  isLoading,
}: AiAnswerPanelProps) {
  const { t, locale } = useText();

  if (isLoading) {
    return (
      <p className="ai-answer__loading">
        <span className="spinner" aria-hidden="true" />
        {t("Asking the AI engine…")}
      </p>
    );
  }

  // Without AI on this instance the form already shows the pointer to the hosted version.
  if (errorCode === AI_UNAVAILABLE) {
    return null;
  }

  if (!answer) {
    return errorCode ? <Notice tone="warning">{t(aiErrorKey(errorCode))}</Notice> : null;
  }

  const seconds = new Intl.NumberFormat(locale, {
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  }).format(answer.latencyMs / 1000);

  return (
    <section className="ai-answer" aria-live="polite">
      <p className="eyebrow ai-answer__eyebrow">
        <i className="bi bi-stars" aria-hidden="true" />
        {t("AI answer")}
      </p>
      <p className="ai-answer__words" lang={language}>
        {answer.words}
      </p>
      <p className="ai-answer__meta">
        <span className={`badge ${answer.agrees ? "badge--success" : "badge--warning"}`}>
          <i
            className={`bi ${answer.agrees ? "bi-check2-circle" : "bi-exclamation-diamond"}`}
            aria-hidden="true"
          />
          {answer.agrees
            ? t("Same answer as the rules engine")
            : t("Different from the rules engine")}
        </span>
        <span>
          {t("Answered by {{model}} in {{seconds}} s", { model: answer.model, seconds })}
        </span>
      </p>
    </section>
  );
}
