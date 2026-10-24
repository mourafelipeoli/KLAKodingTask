import { useId, useState } from "react";
import { textKey } from "../../i18n/textKey";
import { useText } from "../../i18n/useText";
import { ApiError } from "../../services/httpClient";
import { translationAdminService } from "../../services/translationAdminService";
import type { AdminTranslationRow, TranslationSource } from "../../types/translation";

const SOURCE_LABELS: Readonly<Record<TranslationSource, string>> = {
  Baked: textKey("Bundled"),
  Machine: textKey("Machine"),
  Manual: textKey("Edited"),
};

interface TranslationRowEditorProps {
  row: AdminTranslationRow;
  locale: string;
  onSaved: (row: AdminTranslationRow) => void;
}

// One term: the English original, its translation in an editable box (a regional variant
// shows the inherited text as a placeholder), where the translation came from, and whether
// someone has reviewed it. A text changed here becomes an edit the bundled import keeps.
export default function TranslationRowEditor({
  row,
  locale,
  onSaved,
}: TranslationRowEditorProps) {
  const { t } = useText();
  const id = useId();
  const [draft, setDraft] = useState(row.text ?? "");
  const [isReviewed, setIsReviewed] = useState(row.reviewed);
  const [isSaving, setIsSaving] = useState(false);
  const [isSaved, setIsSaved] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const isChanged = draft.trim() !== (row.text ?? "") || isReviewed !== row.reviewed;

  function handleSave() {
    setIsSaving(true);
    setIsSaved(false);
    setErrorMessage(null);
    translationAdminService
      .update({ termId: row.termId, locale, text: draft, reviewed: isReviewed })
      .then((saved) => {
        setIsSaved(true);
        onSaved({ ...saved, inherited: row.inherited });
      })
      .catch((error: unknown) => {
        setErrorMessage(
          error instanceof ApiError && error.status > 0
            ? error.message
            : t("The server is not reachable right now. Try again in a moment."),
        );
      })
      .finally(() => setIsSaving(false));
  }

  return (
    <li className="translation-row">
      <div className="translation-row__original">
        <p className="translation-row__english" lang="en" id={`${id}-english`}>
          {row.english}
        </p>
        <span className={`badge ${row.source ? "badge--muted" : "badge--warning"}`}>
          {t(row.source ? SOURCE_LABELS[row.source] : "Missing")}
        </span>
      </div>
      <label className="visually-hidden" htmlFor={`${id}-text`}>
        {t("Translation")}
      </label>
      <textarea
        id={`${id}-text`}
        className="text-input translation-row__text"
        lang={locale}
        rows={2}
        value={draft}
        placeholder={row.inherited ?? undefined}
        aria-describedby={`${id}-english`}
        onChange={(event) => {
          setDraft(event.target.value);
          setIsSaved(false);
        }}
      />
      <div className="translation-row__actions">
        <label className="translation-row__reviewed">
          <input
            type="checkbox"
            checked={isReviewed}
            onChange={(event) => {
              setIsReviewed(event.target.checked);
              setIsSaved(false);
            }}
          />
          {t("Reviewed")}
        </label>
        <button
          type="button"
          className="button button--secondary button--small"
          disabled={!isChanged || isSaving}
          onClick={handleSave}
        >
          {t("Save")}
        </button>
        <span className="translation-row__status" role="status">
          {isSaved && t("Saved.")}
        </span>
      </div>
      {errorMessage && (
        <p className="field__error translation-row__error" role="alert">
          {errorMessage}
        </p>
      )}
    </li>
  );
}
