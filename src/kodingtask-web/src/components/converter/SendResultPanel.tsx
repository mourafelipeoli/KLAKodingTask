import { useEffect, useRef, useState, useTransition } from "react";
import { useNavigate } from "react-router-dom";
import { useAccount } from "../../app/useAccount";
import { useAppInfo } from "../../app/useAppInfo";
import { useDeployedVersion } from "../../hooks/useDeployedVersion";
import { textKey } from "../../i18n/textKey";
import { useText } from "../../i18n/useText";
import { ROUTES } from "../../routes/routePaths";
import { accountService } from "../../services/accountService";
import { deliveryService } from "../../services/deliveryService";
import { ApiError } from "../../services/httpClient";
import type { ConversionResult, Engine } from "../../types/conversion";
import { DeliveryChannel } from "../../types/conversion";
import { accountErrorKey } from "../../utils/accountErrors";
import { ConverterSection, converterPath } from "../../utils/converterLink";
import Notice from "../Notice";

interface SendResultPanelProps {
  result: ConversionResult;
  engine: Engine;
  /** A channel picked before arriving here (a hand-over or a sign-in on the way). */
  pendingChannel: DeliveryChannel | null;
  onPendingHandled: () => void;
  /** Called with the channel when this copy cannot deliver through it. */
  onUnavailable: (channel: DeliveryChannel) => void;
}

const PHONE_CHECK_NEEDED = "PhoneCheckNeeded";
const PHONE_NOT_VERIFIED = "PhoneNotVerified";
const PHONE_MISSING = textKey(
  "Add and confirm your WhatsApp number in your fmID account, then choose WhatsApp again.",
);

type Status =
  | { kind: "idle" }
  | { kind: "sent"; destination: string; left: number }
  | { kind: "phone-missing" }
  | { kind: "error"; key: string };

// Sends the result to the signed-in visitor. On a copy without a channel the button hands
// over to the deployed version; on the deployed version a channel that is not switched on
// yet is disabled. Everything that needs an account happens on the way and the send
// completes when the visitor comes back: signing in with fmID, and for WhatsApp a quick
// silent check with fmID of the number verified there (the only place a number is managed).
export default function SendResultPanel({
  result,
  engine,
  pendingChannel,
  onPendingHandled,
  onUnavailable,
}: SendResultPanelProps) {
  const { t, locale } = useText();
  const { features, fmIdAccountUrl } = useAppInfo();
  const { isDeployedVersion } = useDeployedVersion();
  const { account, isLoaded, apply } = useAccount();
  const navigate = useNavigate();
  const [status, setStatus] = useState<Status>({ kind: "idle" });
  const [isPending, startTransition] = useTransition();
  const pendingHandled = useRef(false);

  const isAvailable = (channel: DeliveryChannel) =>
    channel === DeliveryChannel.Email ? features.email : features.whatsApp;

  const comeBackPath = (channel: DeliveryChannel | undefined) =>
    converterPath({
      amount: result.amount,
      language: result.language,
      engine,
      uiLocale: locale,
      section: ConverterSection.Send,
      channel,
    });

  function send(channel: DeliveryChannel) {
    startTransition(async () => {
      try {
        const sent = await deliveryService.send(
          channel,
          result.amount,
          result.language,
          locale,
        );
        setStatus({
          kind: "sent",
          destination: sent.maskedDestination,
          left: sent.deliveriesLeftToday,
        });
        await apply({ ...account, deliveriesLeftToday: sent.deliveriesLeftToday });
      } catch (error) {
        if (error instanceof ApiError && error.code === PHONE_CHECK_NEEDED) {
          await accountService.refresh(comeBackPath(channel), locale);
          return;
        }
        setStatus(
          error instanceof ApiError && error.code === PHONE_NOT_VERIFIED
            ? { kind: "phone-missing" }
            : { kind: "error", key: accountErrorKey(error) },
        );
      }
    });
  }

  function choose(channel: DeliveryChannel) {
    const wasPhoneMissing = status.kind === "phone-missing";
    setStatus({ kind: "idle" });
    if (!isAvailable(channel)) {
      if (!isDeployedVersion) {
        onUnavailable(channel);
      }
      return;
    }
    if (!account.isSignedIn) {
      void navigate(
        `${ROUTES.signIn}?returnUrl=${encodeURIComponent(comeBackPath(channel))}`,
      );
      return;
    }
    if (channel === DeliveryChannel.WhatsApp && wasPhoneMissing) {
      // The number may have just been added on fmID: ask fmID again, then send.
      void accountService.refresh(comeBackPath(channel), locale);
      return;
    }
    send(channel);
  }

  // A channel chosen before arriving (on a local copy, or before signing in) completes here
  // once, as soon as the account is known; the address then forgets it.
  useEffect(() => {
    if (!pendingChannel || !isLoaded || pendingHandled.current) {
      return;
    }
    pendingHandled.current = true;
    const address = new URL(window.location.href);
    address.searchParams.delete("send");
    window.history.replaceState(window.history.state, "", address);
    onPendingHandled();
    choose(pendingChannel);
  });

  const channelProps = (channel: DeliveryChannel) => ({
    "data-channel": channel,
    disabled: isPending || (!isAvailable(channel) && isDeployedVersion),
    onClick: () => choose(channel),
  });

  return (
    <section id={ConverterSection.Send} className="send-panel card">
      <div className="send-panel__header">
        <h2 className="send-panel__title">{t("Send this result")}</h2>
        <p className="send-panel__lead">
          {t("Receive the words by email or WhatsApp after signing in.")}
        </p>
      </div>

      <div className="send-panel__channels">
        <button
          type="button"
          className="button button--secondary"
          {...channelProps(DeliveryChannel.Email)}
        >
          <i className="bi bi-envelope" aria-hidden="true" />
          {t("By email")}
        </button>
        <button
          type="button"
          className="button button--secondary"
          {...channelProps(DeliveryChannel.WhatsApp)}
        >
          <i className="bi bi-whatsapp" aria-hidden="true" />
          {t("By WhatsApp")}
        </button>
        {isPending && <span className="spinner" aria-label={t("Sending…")} />}
      </div>

      {account.whatsAppNumber && features.whatsApp && (
        <p className="send-panel__hint">
          {t("Results go to {{number}}.", { number: account.whatsAppNumber })}{" "}
          {fmIdAccountUrl && (
            <a href={fmIdAccountUrl} target="_blank" rel="noopener noreferrer">
              {t("Change it in your fmID account")}
            </a>
          )}
        </p>
      )}

      <div aria-live="polite">
        {status.kind === "sent" && (
          <Notice tone="success">
            {t("Sent to {{destination}}. You can send {{count}} more today.", {
              destination: status.destination,
              count: status.left,
            })}
          </Notice>
        )}
        {status.kind === "phone-missing" && (
          <Notice tone="info">
            {t(PHONE_MISSING)}{" "}
            {fmIdAccountUrl && (
              <a href={fmIdAccountUrl} target="_blank" rel="noopener noreferrer">
                {t("Open your fmID account")}
              </a>
            )}
          </Notice>
        )}
        {status.kind === "error" && (
          <Notice tone="danger" role="alert">
            {t(status.key)}
          </Notice>
        )}
      </div>

      {isDeployedVersion && !features.deliveries && (
        <p className="send-panel__soon">
          {t("Sending by email and WhatsApp is coming soon.")}
        </p>
      )}
    </section>
  );
}
