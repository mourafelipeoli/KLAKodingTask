import { useEffect, useId, useRef } from "react";
import RichText from "../i18n/RichText";
import { useText } from "../i18n/useText";
import "./DeployedVersionDialog.css";

interface DeployedVersionDialogProps {
  isOpen: boolean;
  /** The same page, with the same parameters, on the deployed version. */
  url: string;
  onClose: () => void;
}

// Shown on a copy without credentials (a fresh clone) when the visitor asks for something
// only the deployed version can do: an AI answer, an email, a WhatsApp message. Following
// the link replaces this page with the same one on the deployed version. A native <dialog>
// brings focus trapping, Escape to close and the backdrop for free.
export default function DeployedVersionDialog({
  isOpen,
  url,
  onClose,
}: DeployedVersionDialogProps) {
  const { t } = useText();
  const titleId = useId();
  const dialogRef = useRef<HTMLDialogElement>(null);
  const primaryRef = useRef<HTMLAnchorElement>(null);

  useEffect(() => {
    const dialog = dialogRef.current;
    if (!dialog) {
      return;
    }
    if (isOpen && !dialog.open) {
      dialog.showModal();
      primaryRef.current?.focus();
    } else if (!isOpen && dialog.open) {
      dialog.close();
    }
  }, [isOpen]);

  return (
    <dialog
      ref={dialogRef}
      className="deployed-dialog"
      aria-labelledby={titleId}
      onClose={onClose}
      onClick={(event) => {
        if (event.target === dialogRef.current) {
          dialogRef.current.close();
        }
      }}
    >
      <div className="deployed-dialog__body">
        <i className="bi bi-cloud-check deployed-dialog__icon" aria-hidden="true" />
        <h2 id={titleId} className="deployed-dialog__title">
          {t("Available on the deployed version")}
        </h2>
        <p className="deployed-dialog__text">
          <RichText
            text={t("To access this result, you must use the deployed version {{link}}.")}
            slots={{ link: <a href={url}>{t("here")}</a> }}
          />
        </p>
        <div className="deployed-dialog__actions">
          <a ref={primaryRef} className="button button--primary" href={url}>
            {t("Open the deployed version")}
            <i className="bi bi-arrow-right" aria-hidden="true" />
          </a>
          <button
            type="button"
            className="button button--ghost"
            onClick={() => dialogRef.current?.close()}
          >
            {t("Stay here")}
          </button>
        </div>
      </div>
    </dialog>
  );
}
