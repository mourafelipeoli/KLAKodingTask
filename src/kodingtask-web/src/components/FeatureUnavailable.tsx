import { useAppInfo } from "../app/useAppInfo";
import RichText from "../i18n/RichText";
import { useText } from "../i18n/useText";
import { hostOf } from "../utils/hostOf";
import Notice from "./Notice";

// Shown wherever an optional feature is not configured on this instance (a fresh clone
// has no e-mail, WhatsApp or AI credentials): the feature is still described, and the
// visitor is sent to the hosted version where it works.
export default function FeatureUnavailable() {
  const { t } = useText();
  const { publicSiteUrl } = useAppInfo();

  return (
    <Notice tone="info">
      <RichText
        text={t("To test this feature, use the version published at {{link}}.")}
        slots={{
          link: (
            <a href={publicSiteUrl} target="_blank" rel="noopener">
              {hostOf(publicSiteUrl)}
            </a>
          ),
        }}
      />
    </Notice>
  );
}
