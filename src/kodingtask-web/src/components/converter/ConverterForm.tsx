import { useId } from "react";
import { useText } from "../../i18n/useText";
import type { OutputLanguage } from "../../types/conversion";
import { Engine } from "../../types/conversion";
import FeatureUnavailable from "../FeatureUnavailable";
import EngineSelector from "./EngineSelector";

// The six inputs of the task statement, one click away.
const EXAMPLES = ["0", "1", "25,1", "0,01", "45 100", "999 999 999,99"] as const;

interface ConverterFormProps {
  amount: string;
  language: string;
  languages: readonly OutputLanguage[];
  engine: Engine;
  isAiAvailable: boolean;
  isConverting: boolean;
  errorKey: string | null;
  onAmountChange: (amount: string) => void;
  onLanguageChange: (language: string) => void;
  onEngineChange: (engine: Engine) => void;
  onSubmit: () => void;
  onExample: (amount: string) => void;
}

export default function ConverterForm({
  amount,
  language,
  languages,
  engine,
  isAiAvailable,
  isConverting,
  errorKey,
  onAmountChange,
  onLanguageChange,
  onEngineChange,
  onSubmit,
  onExample,
}: ConverterFormProps) {
  const { t } = useText();
  const amountId = useId();
  const hintId = useId();
  const errorId = useId();
  const languageId = useId();

  return (
    <form
      className="converter"
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit();
      }}
    >
      <div className="converter__fields">
        <div className="field converter__amount">
          <label className="field__label" htmlFor={amountId}>
            {t("Amount in US dollars")}
          </label>
          <div className="converter__input-wrap">
            <span className="converter__currency" aria-hidden="true">
              US$
            </span>
            <input
              id={amountId}
              className="text-input text-input--large converter__input"
              type="text"
              inputMode="decimal"
              autoComplete="off"
              spellCheck={false}
              maxLength={40}
              placeholder="45 100,25"
              value={amount}
              onChange={(event) => onAmountChange(event.target.value)}
              aria-describedby={errorKey ? `${hintId} ${errorId}` : hintId}
              aria-invalid={errorKey ? true : undefined}
            />
          </div>
        </div>

        <div className="field converter__language">
          <label className="field__label" htmlFor={languageId}>
            {t("Output language")}
          </label>
          <select
            id={languageId}
            className="select"
            value={language}
            onChange={(event) => onLanguageChange(event.target.value)}
          >
            {languages.map((option) => (
              <option key={option.code} value={option.code} lang={option.code}>
                {option.nativeName}
              </option>
            ))}
          </select>
        </div>

        <button
          type="submit"
          className="button button--primary button--large converter__submit"
          disabled={isConverting}
        >
          {isConverting ? (
            <>
              <span className="spinner" aria-hidden="true" />
              {t("Converting…")}
            </>
          ) : (
            <>
              {t("Convert")}
              <i className="bi bi-arrow-right" aria-hidden="true" />
            </>
          )}
        </button>
      </div>

      <p id={hintId} className="field__hint">
        {t(
          "Use a comma for the cents and spaces to group thousands, for example 45 100,25.",
        )}
      </p>

      {errorKey && (
        <p id={errorId} className="field__error converter__error" role="alert">
          <i className="bi bi-exclamation-circle" aria-hidden="true" />
          {t(errorKey)}
        </p>
      )}

      <EngineSelector engine={engine} onChange={onEngineChange} />
      {engine === Engine.Ai && !isAiAvailable && <FeatureUnavailable />}

      <div className="converter__examples">
        <span className="converter__examples-label">{t("Try an example:")}</span>
        {EXAMPLES.map((example) => (
          <button
            key={example}
            type="button"
            className="chip"
            onClick={() => onExample(example)}
          >
            {example}
          </button>
        ))}
      </div>
    </form>
  );
}
