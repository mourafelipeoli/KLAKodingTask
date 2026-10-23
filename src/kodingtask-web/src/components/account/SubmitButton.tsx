import { useText } from "../../i18n/useText";

interface SubmitButtonProps {
  label: string;
  isPending: boolean;
}

export default function SubmitButton({ label, isPending }: SubmitButtonProps) {
  const { t } = useText();

  return (
    <button type="submit" className="button button--primary" disabled={isPending}>
      {isPending ? (
        <>
          <span className="spinner" aria-hidden="true" />
          {t("Loading…")}
        </>
      ) : (
        label
      )}
    </button>
  );
}
