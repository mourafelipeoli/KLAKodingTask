import { Link } from "react-router-dom";
import { useDocumentTitle } from "../hooks/useDocumentTitle";
import { useText } from "../i18n/useText";
import { ROUTES } from "../routes/routePaths";

export default function NotFoundPage() {
  const { t } = useText();
  useDocumentTitle(t("Page not found"));

  return (
    <section className="container not-found">
      <p className="eyebrow">404</p>
      <h1 className="not-found__title">{t("Page not found")}</h1>
      <p className="not-found__text">
        {t("The page you are looking for does not exist.")}
      </p>
      <Link className="link-cta" to={ROUTES.home}>
        {t("Back to the converter")}
      </Link>
    </section>
  );
}
