import { useText } from "../../i18n/useText";
import type { ConversionResult } from "../../types/conversion";
import { formatUsd } from "../../utils/formatAmount";
import CopyButton from "../CopyButton";

interface ResultCardProps {
  result: ConversionResult;
  languageName: string;
  isComparisonOpen: boolean;
  onToggleComparison: () => void;
}

function shareLink(result: ConversionResult): string {
  const query = new URLSearchParams({ amount: result.amount, language: result.language });
  return `${window.location.origin}/?${query.toString()}`;
}

export default function ResultCard({
  result,
  languageName,
  isComparisonOpen,
  onToggleComparison,
}: ResultCardProps) {
  const { t, locale } = useText();

  return (
    <section className="result-card" aria-live="polite">
      <p className="eyebrow">{t("In words")}</p>
      <p className="result-card__words" lang={result.language}>
        {result.words}
      </p>
      <p className="result-card__meta">
        <span>
          {t("Understood as {{amount}}", {
            amount: formatUsd(result.dollars, result.cents, locale),
          })}
        </span>
        <span className="result-card__language">
          <i className="bi bi-translate" aria-hidden="true" />
          {languageName}
        </span>
      </p>
      <div className="result-card__actions">
        <CopyButton
          text={result.words}
          label={t("Copy")}
          copiedLabel={t("Copied")}
          icon="bi-clipboard"
        />
        <CopyButton
          text={shareLink(result)}
          label={t("Copy link")}
          copiedLabel={t("Link copied")}
          icon="bi-link-45deg"
        />
        <button
          type="button"
          className="button button--ghost button--small"
          aria-expanded={isComparisonOpen}
          onClick={onToggleComparison}
        >
          <i className="bi bi-columns-gap" aria-hidden="true" />
          {isComparisonOpen ? t("Hide the comparison") : t("Compare all languages")}
        </button>
      </div>
    </section>
  );
}
