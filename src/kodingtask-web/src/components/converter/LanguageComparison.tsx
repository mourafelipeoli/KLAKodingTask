import { useText } from "../../i18n/useText";
import type { ConversionResult } from "../../types/conversion";

interface LanguageComparisonProps {
  results: readonly ConversionResult[] | null;
  isLoading: boolean;
  nameOf: (code: string) => string;
}

export default function LanguageComparison({
  results,
  isLoading,
  nameOf,
}: LanguageComparisonProps) {
  const { t } = useText();

  return (
    <section className="comparison card" aria-busy={isLoading}>
      <h2 className="comparison__title">{t("All languages")}</h2>
      {results === null ? (
        <p className="comparison__loading">
          <span className="spinner" aria-hidden="true" />
          {t("Loading…")}
        </p>
      ) : (
        <dl className="comparison__list">
          {results.map((result) => (
            <div key={result.language} className="comparison__row">
              <dt className="comparison__language">
                {nameOf(result.language)}
                <span className="comparison__code">{result.language}</span>
              </dt>
              <dd className="comparison__words" lang={result.language}>
                {result.words}
              </dd>
            </div>
          ))}
        </dl>
      )}
    </section>
  );
}
