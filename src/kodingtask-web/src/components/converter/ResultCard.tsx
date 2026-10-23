import type { ReactNode } from "react";
import { useText } from "../../i18n/useText";
import type { ConversionResult } from "../../types/conversion";
import type { Engine } from "../../types/conversion";
import { ConverterSection, converterPath } from "../../utils/converterLink";
import { formatUsd } from "../../utils/formatAmount";
import CopyButton from "../CopyButton";

interface ResultCardProps {
  result: ConversionResult;
  engine: Engine;
  languageName: string;
  isComparisonOpen: boolean;
  onToggleComparison: () => void;
  children?: ReactNode;
}

function shareLink(result: ConversionResult, engine: Engine): string {
  const path = converterPath({
    amount: result.amount,
    language: result.language,
    engine,
  });
  return `${window.location.origin}${path}`;
}

export default function ResultCard({
  result,
  engine,
  languageName,
  isComparisonOpen,
  onToggleComparison,
  children,
}: ResultCardProps) {
  const { t, locale } = useText();

  return (
    <section id={ConverterSection.Result} className="result-card" aria-live="polite">
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
      {children}
      <div className="result-card__actions">
        <CopyButton
          text={result.words}
          label={t("Copy")}
          copiedLabel={t("Copied")}
          icon="bi-clipboard"
        />
        <CopyButton
          text={shareLink(result, engine)}
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
