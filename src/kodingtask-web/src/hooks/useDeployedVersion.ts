import { useAppInfo } from "../app/useAppInfo";
import { useText } from "../i18n/useText";

export interface DeployedVersion {
  /** True on the hosted site itself; false on a local or other copy. */
  isDeployedVersion: boolean;
  /** The same page and parameters on the hosted site, in the same interface language. */
  urlFor: (pathAndQuery: string) => string;
}

function originOf(url: string): string | null {
  try {
    return new URL(url).origin;
  } catch {
    return null;
  }
}

// Where the features this copy lacks can be used: the public site from /api/app-info. A copy
// running anywhere else (a clone on a laptop) offers to continue there.
export function useDeployedVersion(): DeployedVersion {
  const { publicSiteUrl } = useAppInfo();
  const { locale } = useText();
  return {
    isDeployedVersion: originOf(publicSiteUrl) === window.location.origin,
    urlFor: (pathAndQuery) => {
      const url = new URL(pathAndQuery, publicSiteUrl);
      if (!url.searchParams.has("lang")) {
        url.searchParams.set("lang", locale);
      }
      return url.toString();
    },
  };
}
