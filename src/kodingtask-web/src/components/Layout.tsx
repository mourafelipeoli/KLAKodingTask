import type { ReactNode } from "react";
import { useText } from "../i18n/useText";
import SiteFooter from "./SiteFooter";
import SiteHeader from "./SiteHeader";

interface LayoutProps {
  children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const { t } = useText();

  return (
    <div className="layout">
      <a className="skip-link" href="#content">
        {t("Skip to content")}
      </a>
      <SiteHeader />
      <main id="content" className="layout__main" tabIndex={-1}>
        {children}
      </main>
      <SiteFooter />
    </div>
  );
}
