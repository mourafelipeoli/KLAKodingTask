import { useAppInfo } from "../app/useAppInfo";
import logoWhite from "../assets/brand/koding-task-logo-white.svg";
import RichText from "../i18n/RichText";
import { useText } from "../i18n/useText";
import "./SiteFooter.css";

export default function SiteFooter() {
  const { t } = useText();
  const { version, repositoryUrl, author } = useAppInfo();

  return (
    <footer className="site-footer">
      <div className="container site-footer__inner">
        <div className="site-footer__brand">
          <img
            className="site-footer__logo"
            src={logoWhite}
            alt="Koding Task"
            width={916}
            height={150}
          />
          <p className="site-footer__tagline">
            {t("Dollar amounts in words, in eight languages.")}
          </p>
        </div>

        <nav className="site-footer__links" aria-label={t("Links")}>
          {author && (
            <a
              className="site-footer__link"
              href={author.url}
              target="_blank"
              rel="noopener"
            >
              <i className="bi bi-person-circle" aria-hidden="true" />
              {t("Portfolio of {{name}}", { name: author.name })}
            </a>
          )}
          {repositoryUrl && (
            <a
              className="site-footer__link"
              href={repositoryUrl}
              target="_blank"
              rel="noopener"
            >
              <i className="bi bi-github" aria-hidden="true" />
              {t("Source code on GitHub")}
            </a>
          )}
          <a className="site-footer__link" href="/openapi/v1.json">
            <i className="bi bi-braces" aria-hidden="true" />
            {t("API documentation (OpenAPI)")}
          </a>
        </nav>

        <div className="site-footer__legal">
          <p className="site-footer__credit">
            {author ? (
              <RichText
                text={t(
                  "Built by {{name}} for the KLA Senior Software Engineer coding task.",
                )}
                slots={{
                  name: (
                    <a href={author.url} target="_blank" rel="noopener">
                      {author.name}
                    </a>
                  ),
                }}
              />
            ) : (
              t("Built for the KLA Senior Software Engineer coding task.")
            )}
          </p>
          {version && (
            <p className="site-footer__version">
              {t("Version {{version}}", { version })}
            </p>
          )}
        </div>
      </div>
    </footer>
  );
}
