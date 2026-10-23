import { useAppInfo } from "../app/useAppInfo";
import { useText } from "../i18n/useText";
import AuthorWordmark from "./AuthorWordmark";
import "./TopBar.css";

// The thin purple strip above the header, as on kla.com: links that lead outside this app,
// here to the author's portfolio and terminal. Hidden when no author is configured.
export default function TopBar() {
  const { t } = useText();
  const { author } = useAppInfo();

  if (!author) {
    return null;
  }

  return (
    <div className="top-bar">
      <nav className="container top-bar__inner" aria-label={t("About the author")}>
        <a
          className="top-bar__link top-bar__link--brand"
          href={author.url}
          target="_blank"
          rel="noopener"
          aria-label={t("Portfolio of {{name}}", { name: author.name })}
          title={t("Portfolio of {{name}}", { name: author.name })}
        >
          <AuthorWordmark name={author.name} />
        </a>
        {author.terminalUrl && (
          <a
            className="top-bar__link"
            href={author.terminalUrl}
            target="_blank"
            rel="noopener"
          >
            <span className="top-bar__prompt" aria-hidden="true">
              &gt;_
            </span>
            {t("Terminal")}
            <i
              className="bi bi-box-arrow-up-right top-bar__external"
              aria-hidden="true"
            />
          </a>
        )}
      </nav>
    </div>
  );
}
