import { useState, useTransition } from "react";
import type { FormEvent } from "react";
import { useAccount } from "../../app/useAccount";
import { useText } from "../../i18n/useText";
import { accountService } from "../../services/accountService";
import { accountErrorKey } from "../../utils/accountErrors";
import Notice from "../Notice";
import AccountField from "../account/AccountField";

interface WhatsAppVerificationProps {
  /** Called once the number is verified; the panel then sends the result. */
  onVerified: () => void;
  onCancel: () => void;
}

// Two small steps inside the send panel: the number, then the code WhatsApp delivered.
export default function WhatsAppVerification({
  onVerified,
  onCancel,
}: WhatsAppVerificationProps) {
  const { t, locale } = useText();
  const { apply } = useAccount();
  const [phone, setPhone] = useState("");
  const [code, setCode] = useState("");
  const [sentTo, setSentTo] = useState<string | null>(null);
  const [errorKey, setErrorKey] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  function handleSendCode(event: FormEvent) {
    event.preventDefault();
    startTransition(async () => {
      try {
        const result = await accountService.sendPhoneCode(phone, locale);
        setSentTo(result.maskedNumber);
        setErrorKey(null);
      } catch (error) {
        setErrorKey(accountErrorKey(error));
      }
    });
  }

  function handleVerify(event: FormEvent) {
    event.preventDefault();
    startTransition(async () => {
      try {
        await apply(await accountService.verifyPhone(code));
        onVerified();
      } catch (error) {
        setErrorKey(accountErrorKey(error));
      }
    });
  }

  return (
    <div className="send-panel__verification">
      {sentTo === null ? (
        <form className="send-panel__form" noValidate onSubmit={handleSendCode}>
          <AccountField
            label={t("WhatsApp number")}
            type="tel"
            inputMode="tel"
            autoComplete="tel"
            hint={t("Use the international format, for example +49 170 1234567.")}
            value={phone}
            onChange={setPhone}
          />
          <div className="send-panel__form-actions">
            <button type="submit" className="button button--primary" disabled={isPending}>
              {t("Send code")}
            </button>
            <button type="button" className="button button--ghost" onClick={onCancel}>
              {t("Cancel")}
            </button>
          </div>
        </form>
      ) : (
        <form className="send-panel__form" noValidate onSubmit={handleVerify}>
          <p className="send-panel__hint">
            {t("We sent a code to {{number}} on WhatsApp.", { number: sentTo })}
          </p>
          <AccountField
            label={t("Code")}
            type="text"
            inputMode="numeric"
            autoComplete="one-time-code"
            value={code}
            onChange={setCode}
          />
          <div className="send-panel__form-actions">
            <button type="submit" className="button button--primary" disabled={isPending}>
              {t("Verify and send")}
            </button>
            <button type="button" className="button button--ghost" onClick={onCancel}>
              {t("Cancel")}
            </button>
          </div>
        </form>
      )}
      {errorKey && (
        <Notice tone="danger" role="alert">
          {t(errorKey)}
        </Notice>
      )}
    </div>
  );
}
